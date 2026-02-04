# THE CHILDREN'S BIBLE

A specification for idiots. Do exactly what it says. Do not improvise.

---

## PART I: OPTIMIZATIONS TO BEAT GCC -O2

You need 80 lines. Two features. Inlining does the heavy lifting. Register allocation is deleted — inlining makes it unnecessary.

---

### 1. INLINING SMALL WORDS (50 lines)

**What it does:** Words under 20 bytes get copied inline instead of called.

**Where to add it:** In `compile-token`, after `dict-find`.

**Current code flow:**
```forth
2dup dict-find ?dup if
  ...
  dict-addr @ gen-call exit   \ <-- THIS IS THE PROBLEM
then
```

**New code flow:**
```forth
2dup dict-find ?dup if
  ...
  dup word-size @ 20 <= if
    dup dict-addr @ inline-code exit   \ COPY CODE, NO CALL
  then
  dict-addr @ gen-call exit
then
```

**Data structures to add:**

```forth
\ In dictionary entry, add word-size field
\ Current: 24 name + 4 addr + 4 flags = 32 bytes
\ New:     24 name + 4 addr + 4 flags + 4 size = 36 bytes
\ OR: steal 4 bits from flags for size (size/4, max 60 bytes)

\ Track code size during compilation
variable word-start  0 word-start !

: start-def ( addr u -- )
  ... existing code ...
  code-here word-start ! ;   \ REMEMBER WHERE WE STARTED

: end-def ( -- )
  ... existing code ...
  code-here word-start @ -   \ SIZE = end - start
  dict-buf dict-count @ 1- 32 * + 28 + ! ;  \ STORE IN ENTRY
```

**The inline-code word:**

```forth
: inline-code ( entry -- )
  dup dict-addr @ swap        \ ( code-addr entry )
  28 + @                      \ ( code-addr size )
  dup 0= if 2drop exit then   \ empty word, skip
  \ Copy bytes from code-addr to current position
  \ BUT: skip the final RET (c3)
  1-                          \ ( code-addr size-1 )
  dup 0<= if 2drop exit then
  0 do
    over i + c@               \ get byte from source
    c,                        \ emit to current position
  loop
  drop ;
```

**What NOT to inline:**
- Words with loops (contain backward jumps)
- Words that call themselves (recurse)
- Words over 20 bytes

**Detection:**

```forth
: has-backward-jump? ( addr size -- flag )
  over + swap do
    i c@ $eb = if                     \ short jmp (2 bytes: eb XX)
      i 1+ c@ dup $80 >= swap $ff <= and   \ signed negative = backward
      if true unloop exit then
    then
    i c@ $e9 = if                     \ near jmp (5 bytes: e9 XX XX XX XX)
      i 4 + c@ $80 and                \ check high byte of 32-bit offset
      if true unloop exit then
    then
  loop false ;

: can-inline? ( entry -- flag )
  dup 28 + @ 20 > if drop false exit then     \ too big
  dup dict-addr @ over 28 + @
  has-backward-jump? if drop false exit then   \ has loops
  drop true ;
```

**CRITICAL: Near-jump offset is little-endian.** The sign bit is in byte 4 (index +4 from opcode), not byte 1.

**Tests to write:**
```
tests/2000-inline-simple.fs      - inline a 3-byte word
tests/2001-inline-no-loop.fs     - do NOT inline word with loop
tests/2002-inline-chain.fs       - inline A which inlines B
tests/2003-inline-size-limit.fs  - do NOT inline 25-byte word
```

---

### 2. STRENGTH REDUCTION (30 lines)

**What it does:** Replace multiply/divide by powers of 2 with shifts.

**Where to add it:** In the `$*` and `$/` handlers in `compile-builtin`.

**Current code:**
```forth
2dup $* str= if 2drop flush-cmp
  ct-depth @ 0= if 0 swap-pending ! else flush-swap then
  ct-depth @ 2 >= if ct-pop ct-pop * ct-push
  else ct-depth @ 1 = if ct-pop flush-pending gen-mul-imm   \ <-- HERE
  else flush-pending gen-mul then then true exit then
```

**New code for multiply:**
```forth
2dup $* str= if 2drop flush-cmp
  ct-depth @ 0= if 0 swap-pending ! else flush-swap then
  ct-depth @ 2 >= if ct-pop ct-pop * ct-push
  else ct-depth @ 1 = if
    ct-pop dup power-of-2? if      \ IS IT 2^N?
      flush-pending
      log2 gen-lshift-imm          \ EMIT: shl rax, N
    else
      flush-pending gen-mul-imm    \ NORMAL PATH
    then
  else flush-pending gen-mul then then true exit then
```

**Helper words:**

```forth
: power-of-2? ( n -- flag )
  dup 0<= if drop false exit then
  dup 1- and 0= ;              \ n & (n-1) == 0 means power of 2

: log2 ( n -- shift )
  0 swap
  begin dup 1 > while
    1 rshift swap 1+ swap
  repeat drop ;

: gen-lshift-imm ( n -- )
  dup 0= if drop exit then        \ shift by 0 = no-op
  dup 1 = if drop                 \ shl rax, 1 = add rax, rax
    $48 c, $01 c, $c0 c,          \ add rax, rax (3 bytes)
  else
    $48 c, $c1 c, $e0 c, c,       \ shl rax, imm8 (4 bytes)
  then ;

: gen-rshift-imm ( n -- )
  dup 0= if drop exit then        \ shift by 0 = no-op
  $48 c, $c1 c, $e8 c, c, ;       \ shr rax, imm8 (4 bytes)
```

**For division — DO NOT APPLY TO SIGNED `/`:**

Signed division rounds toward zero. Shift rounds toward negative infinity. They differ for negative numbers:
```
-7 / 4  = -1  (toward zero)
-7 >> 2 = -2  (toward -inf)
```

**Only apply to `u/` (unsigned division):**
```forth
2dup $u/ str= if 2drop flush-swap
  ct-depth @ 2 >= if ct-pop ct-pop swap u/ ct-push
  else ct-depth @ 1 = if
    ct-pop dup power-of-2? if
      flush-pending
      log2 gen-rshift-imm          \ EMIT: shr rax, N (unsigned shift)
    else
      ct-push ct-flush flush-pending gen-udiv
    then
  else ct-flush flush-pending gen-udiv then then true exit then
```

**Leave signed `/` alone.** Do not optimize it. The idiv instruction is correct.

**For modulo:**
```forth
\ 8 mod  →  7 and  (only for powers of 2)
: gen-and-imm-1 ( n -- )   \ emit: and rax, n-1
  1- gen-and-imm ;
```

**Tests:**
```
tests/2010-strength-mul-2.fs    - 2 * becomes shl 1
tests/2011-strength-mul-8.fs    - 8 * becomes shl 3
tests/2012-strength-div-4.fs    - 4 u/ becomes shr 2
tests/2013-strength-mod-8.fs    - 8 mod becomes 7 and
tests/2014-strength-no-3.fs     - 3 * stays imul (not power of 2)
```

---

### 3. TAIL CALL OPTIMIZATION (already implemented)

The compiler already has tail call optimization. When `recurse` appears at the end of a definition, it becomes `jmp` instead of `call`.

**You do not need to add this.** It exists. See `tail-recurse` variable and `gen-tail-recurse` in sixth.fs.

**Why this matters:** Recursive functions like `fib` benefit. The call overhead disappears for the recursive path.

---

### 4. REGISTER ALLOCATION — DELETED

**Do not implement cross-word register allocation.**

Inlining solves the same problem more simply:
- If a word is small, inline it. No call, no save/restore.
- If a word is large, the save/restore overhead is negligible compared to the work.

Register allocation adds 200 lines of complexity for marginal gain after inlining is done. Delete it.

**The math:**
- Inlining eliminates calls entirely for small words
- Large words have enough work that 2 pushes + 2 pops (8 cycles) are noise
- Tracking clobber sets requires touching every gen-* word
- Not worth it

**Rely on inlining. Skip register allocation.**

---

## PART II: MISSING WORDS FOR SELF-HOSTING

You need 325 lines. Eight groups.

---

### 0. DATA LAYOUT (add first)

Before implementing any words, extend the data segment layout:

```forth
\ Current layout ends at pno-buf (DATA-BASE + 128, 16 bytes)
\ Add runtime variables:
DATA-BASE 144 + constant rt-base    \ runtime BASE variable (for HEX/DECIMAL)
DATA-BASE 152 + constant rt-state   \ runtime STATE variable (for [ ] )
variable data-here  DATA-BASE 160 + data-here !  \ user data starts after
```

**Initialize rt-base to 10 (decimal) at program start.** Add to init-data or equivalent.

---

### 1. HEX (5 lines)

```forth
2dup $hex str= if 2drop flush-swap ct-flush
  $48 c, $c7 c, $04 c, $25 c, rt-base d, 16 d,   \ mov qword [rt-base], 16
  true exit then
```

Add after the `$decimal` handler in `compile-builtin`. Same pattern.

**Test:**
```forth
\ tests/2100-hex.fs
hex 10 . cr      \ should print 16
decimal
```

---

### 2. ALLOT, ALIGN, ALIGNED (20 lines)

**ALLOT** — reserve N bytes in data segment:
```forth
2dup $allot str= if 2drop flush-swap
  ct-depth @ 1 >= if
    ct-pop data-here +!       \ compile-time allot
  else
    \ Runtime allot: add rax to data-here, no codegen needed
    \ This is unusual — allot is typically compile-time only
    ." ALLOT requires literal" cr 1 throw
  then
  true exit then
```

**ALIGNED** — round address up to cell boundary:
```forth
: gen-aligned ( -- )
  \ ( addr -- aligned-addr )
  \ rax = (rax + 7) & ~7
  $48 c, $83 c, $c0 c, 7 c,     \ add rax, 7
  $48 c, $83 c, $e0 c, $f8 c, ; \ and rax, -8

2dup $aligned str= if 2drop flush-swap ct-flush gen-aligned true exit then
```

**ALIGN** — align data-here:
```forth
2dup $align str= if 2drop
  data-here @ 7 + -8 and data-here !    \ -8 = $FFFFFFFFFFFFFFF8 (64-bit)
  true exit then
```

**CRITICAL:** `$FFFFFFF8` is only 32 bits. Use `-8` which sign-extends to full 64-bit mask.

---

### 3. STATE, [, ] (20 lines)

**STATE** — runtime variable for compilation state.

The compiler already has `variable state` for its own use. For the compiled program, use `rt-state` (defined in section 0):

```forth
2dup $state str= if 2drop flush-swap ct-flush
  push-tos
  $48 c, $b8 c, rt-state q,     \ mov rax, rt-state
  true exit then
```

**[** — switch to interpret mode. Compile-time only, immediate:
```forth
2dup $[ str= if 2drop
  0 state !                     \ affects compiler's state, not runtime
  true exit then
```

**]** — switch to compile mode:
```forth
2dup $] str= if 2drop
  1 state !
  true exit then
```

**IMPORTANT:** `[` and `]` are compile-time words. They control the compiler's behavior while compiling. They do not generate runtime code. The `state` variable exposed to the compiled program (`rt-state`) is separate.

---

### 4. UNLOOP (10 lines)

**UNLOOP** — clean up loop indices before EXIT:

```forth
: gen-unloop ( -- )
  \ Drop two items from return stack (loop index and limit)
  $48 c, $83 c, $c4 c, 16 c, ;  \ add rsp, 16

2dup $unloop str= if 2drop flush-swap ct-flush gen-unloop true exit then
```

**Note:** Must be called inside DO...LOOP before EXIT. The generated EXIT does not clean up loop state.

---

### 5. ACCEPT (30 lines)

**ACCEPT** — read line from stdin:
```forth
: gen-accept ( -- )
  \ ( addr maxlen -- len )
  \ addr in rbx, maxlen in rax
  1 has-io !
  \ Save registers
  $53 c,                        \ push rbx (we need it)
  \ syscall: read(0, addr, maxlen)
  $48 c, $89 c, $c2 c,          \ mov rdx, rax (count)
  $48 c, $89 c, $de c,          \ mov rsi, rbx (buffer)
  $48 c, $31 c, $ff c,          \ xor edi, edi (stdin=0)
  $48 c, $31 c, $c0 c,          \ xor eax, eax (sys_read=0)
  $0f c, $05 c,                 \ syscall
  \ Result in rax = bytes read (or -1 on error)
  \ Newline included in count — caller handles it
  $5b c,                        \ pop rbx (restore)
  pop-nos ;                     \ consumed 2, produced 1

2dup $accept str= if 2drop flush-swap ct-flush flush-pending gen-accept true exit then
```

---

### 6. CREATE and DOES> (80 lines)

**This is the hardest part. Do not skip any step.**

**CREATE** — make a new dictionary entry at runtime:

This is complex because the compiler runs at compile time, but CREATE runs at runtime in the compiled program. You need runtime dictionary management.

**Simpler approach for compiler:** CREATE is compile-time only.

```forth
2dup $create str= if 2drop
  get-token                     \ get the name
  dup 0= if 2drop ." Expected name after CREATE" cr 1 throw then
  dict-add                      \ add to dictionary
  \ Default behavior: push data field address
  push-tos
  $48 c, $b8 c, data-here @ q,  \ mov rax, data-field-addr
  $c3 c,                        \ ret
  \ Mark as CREATE'd word (for DOES> to patch)
  $1000 dict-buf dict-count @ 1- 32 * + dict-flags @ or
  dict-buf dict-count @ 1- 32 * + dict-flags !
  true exit then
```

**DOES>** — define runtime behavior for CREATE'd words:

DOES> has two parts:
1. At compile time: end the current definition's "build" code, start the "does" code
2. At runtime (when the defining word runs): patch the last CREATE'd word to jump to the "does" code

```forth
variable does-addr  0 does-addr !

2dup $does> str= if 2drop flush-swap ct-flush
  \ Compile code that will patch LATEST at runtime
  \ First: emit call to (does>) helper
  s" (does>)" dict-find ?dup if
    dict-addr @ gen-call
  else
    ." (does>) not defined" cr 1 throw
  then
  \ Save current position as does-body start
  code-here does-addr !
  \ Continue compiling does-body
  true exit then
```

**The (does>) runtime helper:**

```forth
: (does>) ( -- )
  \ Called at runtime when defining word executes
  \ R: has return address = does-body location
  \ Patch LATEST's code to: push DFA; jmp does-body

  \ This requires runtime dictionary access
  \ For a compile-time-only system, this is complex

  \ SIMPLIFICATION: does> only works at compile time
  ;
```

**REAL SIMPLIFICATION:**

For the compiler, implement CREATE...DOES> as a compile-time macro pattern:

```forth
\ When we see:  : CONSTANT  CREATE , DOES> @ ;
\ We compile it as if it were:
\ : CONSTANT ( n -- )
\   CREATE ,             \ build phase
\   ['] @-dfa-thunk      \ does> phase: inline the @ with DFA on stack
\ ;

\ And we generate specialized code for each CONSTANT:
\ CONSTANT FOO generates:
\   FOO: mov rax, [foos-data-field] ; ret
```

This avoids runtime dictionary patching. Each use of CONSTANT/VARIABLE/etc. generates optimal code at compile time.

**Minimal working DOES>:**

```forth
\ Track the last CREATE'd word
variable latest-create  0 latest-create !

: $create-handler ( -- )
  get-token
  dict-add
  dict-count @ 1- 32 * dict-buf + latest-create !
  \ Emit placeholder code (will be patched by DOES>)
  push-tos
  $48 c, $b8 c,                 \ mov rax, imm64
  code-here latest-create @ 24 + !   \ save code position for patching
  data-here @ q,                \ data field address
  $c3 c, ;                      \ ret

: $does>-handler ( -- )
  \ Get return address from caller = does-body start
  \ Patch latest-create's code to jump there instead of ret

  \ At compile time, we're inside the defining word
  \ We need to emit code that:
  \ 1. Gets the does-body address (next instruction after does>)
  \ 2. Patches latest-create to jump there

  \ Emit: call does-patcher; <does-body follows>
  \ does-patcher pops return addr, patches latest, returns to after does-body

  \ This is getting complicated. Let's just inline it.

  \ SIMPLEST: does> ends the create part, starts inlining does code
  code-here does-addr !
  \ When we see ;, we'll patch latest-create to jump to does-addr
  ;
```

**Actually, just do this:**

```forth
\ For now, implement only CONSTANT and VARIABLE as builtins.
\ CREATE and DOES> are for the full interpreter.
\ The compiler can bootstrap without them.
```

If you need CREATE/DOES>, implement after everything else works.

---

### 7. FIND, ', ['], EXECUTE, >BODY (60 lines)

**These require runtime dictionary access.**

For a compile-time-only compiler, these are not needed. The compiled program does not have a dictionary.

**If you need them:**

You must embed the dictionary into the compiled program's data segment, and implement these as runtime words.

**Simpler: Skip for now.** The Hayes tests use these, but you can implement them in the Forth interpreter (which is written in Forth and compiled by this compiler).

---

### 8. EVALUATE, SOURCE, >IN, WORD, PARSE (100 lines)

**These require runtime parsing.**

Same issue: the compiler parses at compile time. The compiled program does not parse.

**For self-hosting:** The interpreter (written in Forth) implements these. The compiler just needs to compile the interpreter's source code.

**The interpreter needs:**
- Input buffer (already have: input-buf)
- Parse position (already have: input-pos)
- The ability to read and execute tokens

**EVALUATE implementation (in the interpreter, not compiler):**

```forth
\ This is Forth code that the compiler will compile
variable source-addr
variable source-len
variable >in

: source ( -- addr len ) source-addr @ source-len @ ;

: parse-name ( -- addr len )
  \ Skip spaces
  begin
    >in @ source-len @ < if
      source-addr @ >in @ + c@ bl <=
    else false then
  while
    1 >in +!
  repeat
  \ Mark start
  source-addr @ >in @ +
  0
  \ Collect non-spaces
  begin
    >in @ source-len @ < if
      source-addr @ >in @ + c@ bl >
    else false then
  while
    1 >in +! 1+
  repeat ;

: evaluate ( addr len -- )
  source-len @ >r source-addr @ >r >in @ >r
  source-len ! source-addr ! 0 >in !
  begin
    parse-name dup
  while
    find-and-execute   \ you implement this
  repeat
  2drop
  r> >in ! r> source-addr ! r> source-len ! ;
```

**The compiler's job:** Compile this Forth code into native code. The compiler does not need to understand EVALUATE semantically. It just compiles it.

---

## PART III: TESTING

Write tests as you go. Do not batch.

**Test file format:**
```forth
\ tests/NNNN-name.fs
\ Expected output: <expected>

... test code ...
bye
```

**Run a test:**
```bash
./engine/fifth tests/2000-inline-simple.fs > /tmp/out 2>&1
./out
```

**Test numbering:**
- 2000-2099: Inlining
- 2100-2199: Strength reduction
- 2200-2299: (reserved)
- 2300-2399: Missing words

---

## PART IV: ORDER OF IMPLEMENTATION

0. **Data layout** (5 lines) — Define rt-base, rt-state. Do this FIRST.
1. **Strength reduction** (30 lines) — Easiest. Pattern match on literals.
2. **Inlining** (50 lines) — Big win. Eliminates call overhead.
3. **HEX** (5 lines) — Copy decimal, change 10 to 16.
4. **ALLOT/ALIGN/ALIGNED** (20 lines) — Simple data segment math.
5. **UNLOOP** (10 lines) — Single instruction.
6. **STATE/[/]** (15 lines) — Variables and flags.
7. **ACCEPT** (30 lines) — Syscall wrapper.
8. **CREATE/DOES>** (80 lines) — Only if needed for interpreter.
9. **FIND/EXECUTE/etc.** (60 lines) — Only if needed.
10. **EVALUATE/PARSE/etc.** (100 lines) — Implement in Forth, not compiler.

**Register allocation is deleted.** Inlining makes it unnecessary.

**Total to beat GCC:** 80 lines (strength + inlining).
**Total for useful self-hosting:** 170 lines.
**Total for full interpreter:** 410 lines.

---

## PART V: DEBUGGING

**When things break:**

1. **Segfault on run:** Stack imbalance. Add `.s` after every word.
2. **Wrong output:** Constant folding bug. Add `ct-depth @ . cr` in compile-builtin.
3. **Infinite loop:** Loop elimination broke. Check gen-repeat byte patterns.
4. **Call to wrong address:** Inlining copied too much/little. Check word-size.

**The nuclear option:**

```forth
\ Disable ALL optimizations:
: ct-push drop ;
: ct-pop 0 ;
: ct-flush ;
: ct-depth 0 ;
```

If it works with optimizations disabled, you broke an optimization.
Binary search to find which one.

---

## SUMMARY

| Task | Lines | Difficulty | Do it? |
|------|-------|------------|--------|
| Data layout | 5 | Trivial | YES (first) |
| Strength reduction | 30 | Easy | YES |
| Inlining | 50 | Medium | YES |
| Tail call optimization | 0 | Done | ALREADY EXISTS |
| Register allocation | 0 | Skip | DELETED — inlining covers it |
| HEX | 5 | Trivial | YES |
| ALLOT/ALIGN/ALIGNED | 20 | Easy | YES |
| UNLOOP | 10 | Trivial | YES |
| STATE/[/] | 15 | Easy | YES |
| ACCEPT | 30 | Easy | YES |
| CREATE/DOES> | 80 | Hard | LATER |
| FIND/EXECUTE | 60 | Medium | LATER |
| EVALUATE/PARSE | 100 | In Forth | LATER |

**To beat GCC:** 80 lines (inlining + strength reduction).
**For useful self-hosting:** 170 lines (above + basic words).
**Full system:** 410 lines.

This is the specification. Follow it exactly. Do not improvise. When you finish, you will have a compiler that beats GCC -O2 in ~2850 lines of Forth.

That is the point.
