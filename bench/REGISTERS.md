# Register Allocation in Sixth: 3 Registers vs 15 GPRs

## The Problem

The `spill` benchmark runs **18x slower** on tf than gcc -O2. Both compute the same thing: four variables rotated and accumulated in a tight loop. gcc keeps all four in registers. tf can only cache three stack items in registers — the fourth spills to memory on every access.

This document explains why tf has 3 register-cached slots, why gcc has 15, what happens when you exceed 3, and what it would take to add more.

---

## x86-64 Register Inventory

x86-64 has 16 general-purpose 64-bit registers:

| Register | sixth.fs usage | gcc -O2 usage | Notes |
|----------|-------------|---------------|-------|
| **rax** | TOS (top of stack) | General purpose | Clobbered by `mul`, `div` (low result) |
| **rbx** | NOS (2nd on stack) | General purpose | Callee-saved |
| **rcx** | 3rd on stack | General purpose | Required for shift count (`shl cl`) |
| **rdx** | *unused / clobbered* | General purpose | Clobbered by `mul`, `div` (high result / remainder) |
| **rsi** | *unused* | General purpose | |
| **rdi** | *unused* | General purpose | |
| **rbp** | Return stack pointer | Frame pointer | tf uses for Forth return stack at 0x40F000 |
| **rsp** | x86 call stack | x86 call stack | Used by `call`/`ret` |
| **r8** | *unused* | General purpose | REX prefix required |
| **r9** | *unused* | General purpose | REX prefix required |
| **r10** | *unused* | General purpose | REX prefix required |
| **r11** | *unused* | General purpose | REX prefix required |
| **r12** | do/loop index (`i`) | General purpose | REX prefix, callee-saved |
| **r13** | do/loop limit | General purpose | REX prefix, callee-saved |
| **r14** | *unused* | General purpose | REX prefix, callee-saved |
| **r15** | Data stack pointer | General purpose | REX prefix, callee-saved |

**gcc's count**: 16 total − rsp (call stack) = 15 usable. In practice gcc uses 13-14 because it reserves rbp as frame pointer in debug builds, but at -O2 it omits the frame pointer and uses all 15.

**tf's count**: rax + rbx + rcx = **3 register-cached stack slots**. The other 13 registers are either dedicated (r15=stack pointer, rbp=return stack, r12/r13=loop vars, rsp=call stack) or unused (rdx, rsi, rdi, r8-r11, r14).

---

## Why tf Uses Only 3

### 1. Historical progression

From PERF.md, the compiler evolved through three stages:

| Stage | Registers | Benchmark | Improvement |
|-------|-----------|-----------|-------------|
| Original | rax only (TOS) | 0.065s | baseline |
| + rbx | rax + rbx | — | ~1.5x |
| + rcx (depth tracking) | rax + rbx + rcx | 0.033s | 2x over original |

Adding the second register (rbx for NOS) was the single biggest win. Adding the third (rcx) gave another significant improvement because `swap over +` (the fibonacci pattern) became 100% register-based. The depth tracking system was the enabling mechanism — the compiler tracks stack depth at compile time and emits different code depending on whether depth is 1, 2, 3, or 4+.

### 2. The stack machine model

tf is a **stack compiler**, not a register allocator. It doesn't analyze which values are live, which are dead, or which registers are free. It maintains a single integer (`stack-depth`) and maps stack positions to fixed registers:

```
Position 0 (TOS) → always rax
Position 1 (NOS) → always rbx
Position 2       → always rcx
Position 3+      → always memory at [r15], [r15+8], ...
```

This is a positional mapping, not an allocation. Every `push` shifts the entire register chain down. Every `pop` shifts it up. There is no concept of "this value lives in r8 for the next 20 instructions."

### 3. Instruction conflicts on rcx and rdx

**rcx** is the shift count register. x86 requires that `shl` and `shr` use the `cl` register (low byte of rcx). When tf compiles `lshift` or `rshift` at depth ≥ 3, it must save rcx to the x86 stack, use it for the shift, then restore it:

```asm
push rcx          ; save 3rd stack item
mov  cl, al       ; shift count from TOS
shl  rbx, cl      ; shift NOS by count
pop  rcx          ; restore 3rd item
```

If rcx held a 4th or 5th stack item, more saves/restores would be needed. The conflict cost grows with depth.

**rdx** is clobbered by `mul` and `div`. The x86 `idiv` instruction divides rdx:rax by the operand, putting the quotient in rax and remainder in rdx. If rdx held a stack item, every multiply or divide would need to save and restore it. tf avoids this by not allocating rdx.

### 4. REX prefix overhead

Instructions that reference r8-r15 require a REX prefix byte (`0x41`, `0x49`, etc.), making every instruction 1 byte longer. For registers rax-rdi, the REX.W prefix (`0x48`) is already needed for 64-bit operand size, but no additional encoding is required. For r8-r15, you need both REX.W and REX.B or REX.R bits.

Example — `mov rax, rbx` vs `mov rax, r8`:

```
48 89 d8        mov rax, rbx     (3 bytes)
4c 89 c0        mov rax, r8      (3 bytes — same size here, but...)
49 89 07        mov [r15], rcx   (3 bytes — using r15 base)
```

The encoding overhead is small (0-1 bytes per instruction) but nonzero. In a tight loop emitting 2-3 instructions per iteration, every byte matters for instruction cache pressure.

### 5. Diminishing returns

Most Forth words operate at stack depth 1-3. The standard Forth idioms:

```forth
dup . drop              ( depth 1-2 )
swap over +             ( depth 2-3 )
rot                     ( depth 3 )
1+ 1-                   ( depth 1 )
! @                     ( depth 1-2 )
```

Depth 4+ occurs in specific patterns: `2dup 2over`, deep stack juggling, or (as in `spill.fs`) multiple variable accesses that pile up on the stack. These are the minority case.

The 3-register model covers the common case. Going to 4 registers catches `2dup 2over` and similar. Going to 5+ catches almost nothing that regular Forth code produces.

---

## What Happens at Depth 4+

When a fourth item is pushed, `push-tos` executes:

```asm
sub  r15, 8          ; make room on memory stack
mov  [r15], rcx      ; spill 3rd item (rcx) to memory
mov  rcx, rbx        ; shift rbx → rcx
mov  rbx, rax        ; shift rax → rbx
; (new value loaded into rax)
```

When it's later popped, `pop-tos` executes:

```asm
mov  rax, rbx        ; shift rbx → rax
mov  rbx, rcx        ; shift rcx → rbx
mov  rcx, [r15]      ; reload from memory
add  r15, 8          ; shrink memory stack
```

**Cost per spill/reload cycle**: 7 instructions, 2 memory operations. On modern x86, each memory access is 4-5 cycles on L1 cache hit. The register-to-register moves are 0-1 cycles (often eliminated by register renaming).

In the `spill` benchmark, every iteration does 4 variable loads (`@`) and 4 variable stores (`!`), each of which pushes/pops the stack. With depth hovering around 2-4, roughly half the operations trigger spills. That's ~8 spill/reload cycles × 7 instructions = **56 extra instructions per iteration** versus gcc's zero.

---

## The spill Benchmark in Detail

### Forth version (spill.fs)

```forth
variable va  variable vb  variable vc  variable vd
: main ( -- )
  1 va !  2 vb !  3 vc !  4 vd !
  100000000 0 do
    va @ vd @ + va !       \ a += d
    vb @ vc @ + vb !       \ b += c
    va @ vb @ + vc !       \ c = a + b
    vc @ vd @ + vd !       \ d = c + d
  loop
  vd @ . cr ;
```

Each `va @` pushes the variable address, then loads from it. Each `va !` stores TOS to the address, then pops both. The stack depth oscillates between 1 and 3 during each line, but the variable addresses themselves consume stack slots, temporarily pushing depth to 3-4.

### C version (bench.c)

```c
NOINLINE int64_t bench_spill(void) {
    int64_t a = 1, b = 2, c = 3, d = 4;
    for (int64_t i = 0; i < 100000000; i++) {
        a += d; b += c; c = a + b; d = c + d;
        KEEP(a); KEEP(b); KEEP(c); KEEP(d);
    }
    return d;
}
```

gcc -O2 output (approximate):

```asm
.loop:
    add rsi, rcx       ; a += d
    add rdx, rbx       ; b += c
    lea rbx, [rsi+rdx] ; c = a + b
    lea rcx, [rbx+rcx] ; d = c + d
    dec rdi
    jnz .loop
```

**6 instructions, zero memory traffic.** All four variables live in registers for the entire loop. The 4 KEEP() clobbers prevent gcc from further optimizing but don't cause memory access — they just make the values opaque to the optimizer.

### The gap

| | tf | gcc -O2 | Ratio |
|--|-----|---------|-------|
| Instructions per iteration | ~60 | 6 | 10x |
| Memory operations per iteration | ~16 | 0 | ∞ |
| Runtime (100M iterations) | 1111ms | 61ms | 18.2x |

The 10x instruction ratio doesn't fully explain the 18x wall-clock gap. The extra factor comes from memory latency (even L1 hits are 4-5 cycles vs 0 for register ops) and pipeline stalls from data dependencies through memory.

---

## Options for Adding More Register-Cached Slots

### Option A: 4 registers (add rdx)

**Assign rdx to stack position 3. Move rcx to position 2 only.**

| Slot | Register |
|------|----------|
| TOS | rax |
| NOS | rbx |
| 3rd | rcx |
| 4th | rdx |

**Advantages:**
- Catches the 4-variable case (spill.fs would improve dramatically)
- `2dup 2over` stays in registers
- rdx is a "legacy" register — no REX.B prefix needed
- Minimal encoding overhead

**Disadvantages:**
- `mul` clobbers rdx. Every `*` must save/restore rdx when depth ≥ 4
- `div` clobbers rdx. Same problem for `/`, `mod`, `fm/mod`
- Every arithmetic operation that might clobber rdx needs a depth check
- `push-tos` and `pop-tos` become 4-way cascades instead of 3-way
- The multiply/divide save/restore cost may negate the register benefit in code that mixes arithmetic with deep stacks

**Estimated complexity**: Moderate. Every occurrence of `push-tos`, `pop-tos`, `pop-nos`, `gen-div`, `gen-mul`, `gen-mod` needs an additional depth branch. Roughly 30-40 lines of additional machine code emission.

**Estimated benefit**: spill.fs drops from 18x to ~5-8x (still slower than gcc because variable access patterns differ from register allocation). Benchmarks that stay at depth ≤ 3 see zero change.

### Option B: 5-6 registers (add rsi, rdi)

**Assign rsi and rdi to stack positions 4-5.**

| Slot | Register |
|------|----------|
| TOS | rax |
| NOS | rbx |
| 3rd | rcx |
| 4th | rdx |
| 5th | rsi |
| 6th | rdi |

**Advantages:**
- Covers essentially all realistic Forth stack depths
- rsi/rdi have no special x86 semantics (no clobber by any instruction)
- No REX.B prefix needed
- The `spill` benchmark would run at near-gcc speed

**Disadvantages:**
- `push-tos` becomes a 6-way cascade: every push shifts 5 registers
- At depth 6, a push emits: `sub r15,8; mov [r15],rdi; mov rdi,rsi; mov rsi,rdx; mov rdx,rcx; mov rcx,rbx; mov rbx,rax` — **7 instructions** just to make room for one value
- `drop` similarly shifts 5 registers back up
- `swap` is still just `xchg rax, rbx` (no change)
- `rot` touches rax/rbx/rcx (no change)
- But `dup` at depth 5 emits 7 instructions vs 5 at depth 3
- The register cascade becomes the bottleneck instead of memory access
- **Net effect may be negative** for typical code: more register shuffling instructions even when the program rarely exceeds depth 3

**Estimated complexity**: High. Every stack manipulation word needs 6 depth branches. The machine code emission becomes significantly harder to verify. Testing combinatorics increase.

**Estimated benefit**: spill.fs approaches gcc parity. Everything else gets marginally slower due to longer push/pop sequences at lower depths (the extra depth checks and conditional emissions add code, even if the branches aren't taken at runtime — they're taken at compile time, making compilation slower).

### Option C: Extended registers (r8-r11)

**Use r8-r11 for positions 4-7.**

| Slot | Register |
|------|----------|
| TOS | rax |
| NOS | rbx |
| 3rd | rcx |
| 4th | r8 |
| 5th | r9 |
| 6th | r10 |
| 7th | r11 |

**Advantages:**
- r8-r11 are caller-saved (like rax-rdx), so no callee-save obligations
- No conflicts with `mul`/`div`/`shl` (unlike rdx/rcx)
- 7 register slots covers any realistic Forth program

**Disadvantages:**
- Every instruction referencing r8-r11 needs REX.B prefix: +1 byte per instruction
- The push/pop cascade is now 7-way: `push-tos` at depth 7 emits ~9 instructions
- In a tight loop with frequent dup/drop, the cascade overhead dominates
- Instruction cache pressure increases: more bytes emitted per word
- r12/r13 already used for do/loop — if nested do/loop is ever extended (j word, nested limits), those registers are taken
- **tf emits machine code bytes directly** (no assembler). Every new register means new encoding tables, new REX byte combinations, new ModR/M patterns. The manual encoding complexity roughly doubles.

**Estimated complexity**: Very high. The x86 instruction encoding for r8-r15 is different enough that nearly every code generation word needs a second code path. This is the most invasive change.

**Estimated benefit**: Same as Option B for runtime. Worse for code size.

### Option D: Variable-to-register promotion (different approach)

Instead of adding more stack-cached slots, **detect variables used in loops and pin them to registers**.

```forth
\ Compiler detects: va, vb, vc, vd are the only variables in this loop
\ Assigns: va→r8, vb→r9, vc→r10, vd→r11
\ va @ compiles to: push-tos; mov rax, r8  (no memory access)
\ va ! compiles to: mov r8, rax; pop-tos   (no memory access)
```

**Advantages:**
- Solves the spill problem at its root: variables live in registers, not memory
- Stack depth stays at 1-3 (the cascade problem doesn't apply)
- Only affects code inside loops — no overhead elsewhere
- This is what gcc actually does (liveness analysis + register allocation)

**Disadvantages:**
- Requires dataflow analysis: which variables are accessed in the loop? How many? Are there conflicts?
- Requires a second compilation pass (or lookahead): scan the loop body before emitting code
- Must handle nested loops, calls within loops, and early exits
- The compiler currently works in a single forward pass — this would fundamentally change the architecture
- If the loop uses more variables than available registers, need a spill strategy (which is what we're trying to avoid)

**Estimated complexity**: Very high. This is the difference between a "smart assembler" and a real optimizing compiler. It requires IR (intermediate representation), live range analysis, and graph coloring or linear scan allocation.

**Estimated benefit**: Potentially closes the gap to 1-2x for all register-pressure benchmarks. But the implementation cost is an order of magnitude beyond any other option.

---

## Comparison of Options

| Option | Registers | spill.fs improvement | Typical code impact | Complexity | Code size impact |
|--------|-----------|---------------------|---------------------|------------|-----------------|
| Current | 3 (rax, rbx, rcx) | baseline (18x) | baseline | — | — |
| A: +rdx | 4 | ~5-8x | neutral | moderate | +5% |
| B: +rsi,rdi | 6 | ~2-3x | slightly negative | high | +15% |
| C: +r8-r11 | 7 | ~2-3x | negative (encoding) | very high | +25% |
| D: var promotion | 3 + promoted | ~1-2x | positive | extreme | +10% |

---

## Recommendation

**Option A (add rdx as 4th register)** is the only change with a favorable cost-benefit ratio. It catches the most common spill case (depth 4) with moderate implementation effort and no impact on the majority of code that stays at depth ≤ 3.

The mul/div conflict with rdx is manageable because:
1. Multiply and divide are infrequent compared to add/sub/compare
2. The save/restore cost (push rdx; ...; pop rdx) is 2 instructions — far cheaper than the ~56 instructions saved by avoiding spills in a 4-variable loop
3. The depth check is compile-time, not runtime — zero cost when depth < 4

Options B and C suffer from the cascade problem: the more registers in the chain, the more instructions each push/pop emits. At 6-7 registers, the push cascade itself becomes a performance bottleneck, potentially making code slower even when no spills occur.

Option D is the right long-term answer but is a fundamentally different compiler architecture. It belongs in a rewrite, not an incremental improvement.

---

## What the Benchmarks Show

From bench/BENCHMARKS.md (2026-02-01):

| Depth pattern | Benchmark | tf/gcc-O2 | Registers used |
|---------------|-----------|-----------|----------------|
| depth ≤ 2 | stack (swap) | **0.54x** (tf wins) | rax, rbx |
| depth ≤ 2 | loop-std | **0.96x** (parity) | rax |
| depth ≤ 3 | nested | **0.65x** (tf wins) | rax + r12/r13 |
| depth ≤ 3 | fib (tuck+) | **1.09x** (near parity) | rax, rbx, rcx |
| depth ≤ 3 | fib-std (swap over +) | **1.27x** | rax, rbx, rcx |
| depth 4+ | spill | **18.21x** | rax, rbx, rcx + memory |
| depth 4+ | mem | **5.44x** | rax, rbx + memory |

The pattern is unambiguous: **at depth ≤ 3, tf is competitive with or beats gcc -O2. At depth 4+, tf falls off a cliff.** The 3-register boundary is the single biggest determinant of tf's performance relative to gcc.

---

## Appendix: Register Allocation in sixth.fs

### Current register map

```
rax  — TOS (always)
rbx  — NOS (depth ≥ 2)
rcx  — 3rd  (depth ≥ 3)
rdx  — unused (clobbered by mul/div, used transiently in fm/mod)
rsi  — unused
rdi  — unused
rbp  — Forth return stack pointer (>r, r>, 2>r, 2r>)
rsp  — x86 call stack (call/ret)
r8   — unused
r9   — unused
r10  — unused
r11  — unused
r12  — do/loop index (i)
r13  — do/loop limit
r14  — unused
r15  — data stack pointer (memory spills)
```

### push-tos (shift chain)

```
depth ≥ 3: sub r15, 8; mov [r15], rcx   (spill rcx to memory)
depth ≥ 2: mov rcx, rbx                  (shift NOS to 3rd)
always:    mov rbx, rax                   (shift TOS to NOS)
           ; new value loaded into rax
           stack-depth++
```

### pop-tos (reverse shift chain)

```
always:    mov rax, rbx                   (shift NOS to TOS)
depth ≥ 2: mov rbx, rcx                  (shift 3rd to NOS)
depth ≥ 3: mov rcx, [r15]; add r15, 8    (reload from memory)
           stack-depth--
```

### Memory layout

```
0x400000 - 0x401FFF : ELF header + code (.text)
0x402000 - 0x403FFF : String data
0x404000 - 0x407FFF : Variables and memory (! @ c! c@)
0x408000 ← r15      : Data stack (grows downward)
0x40F000 ← rbp      : Return stack (grows downward)
```
