# Spill/Fill Optimization: Tradeoffs

## Current Architecture

Sixth uses a simple register model:
```
rax = TOS (top of stack)
rbx = NOS (next on stack)
rcx = 3rd
r15 = data stack pointer (depth > 3 spills here)
rbp = return stack pointer
```

No callee-saved registers. No register allocation. Stack depth tracked at compile time.

## What Spill/Fill Requires

### Register Reservation

Need r12-r14 for recursion cache. Three options:

**Option A: Steal from nowhere**
r12-r14 currently unused. Just take them.
- Cost: None immediate
- Risk: Closes door on future optimizations that might want them

**Option B: Move data stack**
Free r15 for cache by moving data stack to r14.
- Cost: Change every `r15` reference in codegen
- Risk: Subtle bugs, larger diffs

**Option C: Use x86 stack**
Cache frames on x86 stack (rsp) instead of registers.
- Cost: Slower than registers
- Risk: Conflicts with current rsp usage (syscalls, I/O)

### Calling Convention Change

Current: Caller saves nothing. All registers volatile.

With cache: r12-r14 must survive across calls.

```
; BEFORE: any call
call foo        ; rax/rbx/rcx/r12-r15 all trashed

; AFTER: calls must preserve r12-r14
push r12
push r13
push r14
call foo
pop r14
pop r13
pop r12
```

This applies to ALL calls, not just recursive ones. Every `gen-call` grows by 6 instructions.

**Or**: Only recursive words use the cache. Non-recursive words ignore r12-r14.

Problem: How does the compiler know if a word is recursive? Currently doesn't track this.

### Compiler Complexity

Current word compilation:
```
1. Parse
2. Generate code
3. Done
```

With recursion optimization:
```
1. Parse
2. Detect if recursive
3. Detect if double-recursive
4. If yes: generate loop + cache + spill logic
5. If no: generate normal code
6. Track cache state through control flow
7. Done
```

New state to track:
- Is current word recursive?
- Is it double-recursive?
- Which cache registers in use?
- Where are spill points?

### Impact on Existing Optimizations

**Constant folding**: Unaffected.

**Literal fusion**: Unaffected.

**Tail-call**: Conflicts. Tail-call uses `jmp`. Cache loop also uses `jmp`. Need to distinguish.

**Swap absorption**: Must flush before cache operations.

**Dup+cmp fusion**: Must flush before cache operations.

**Register stack**: Cache registers reduce available registers for stack caching. If r12-r14 are cache, can't use them for deep stack.

### Code Size

Current compiler: 2600 lines.

Spill/fill optimization: +150-200 lines.

New complexity:
- Pattern matcher for double-recurse
- Cache register allocator
- Spill point detector
- Fill point generator
- Integration with existing control flow

### Testing

New test cases needed:
- Single recursion (should not use cache)
- Double recursion (should use cache)
- Triple recursion (should use cache)
- Recursion depth > cache size (must spill correctly)
- Recursion inside loops
- Recursion inside conditionals
- Mutual recursion (two words calling each other)

Mutual recursion is particularly tricky. Cache belongs to which word?

## The Real Question

Is 4.4x slowdown on Ackermann worth 200 lines and architectural complexity?

**Arguments for:**
- Proves the compiler can compete with GCC
- Deep recursion exists in real code (tree traversal, parsing)
- Educational value

**Arguments against:**
- Ackermann is pathological
- Real code uses loops, not 3-million-deep recursion
- Primes benchmark only 1.7x slower — that's typical code
- 200 lines is 8% growth in compiler size
- Complexity breeds bugs

## Minimal Alternative

Instead of compiler optimization, document the pattern:

```forth
\ SLOW: deep double recursion
: ack ( m n -- r )
  over 0= if nip 1+ exit then
  dup 0= if drop 1- 1 recurse exit then
  over 1- -rot 1- recurse recurse ;

\ FAST: explicit stack, no recursion
: ack-iter ( m n -- r )
  ... iterative implementation with manual stack ...
```

Let the programmer choose. Forth philosophy: simple tools, smart programmer.

## Decision

Document as future optimization. Do not implement unless:
1. Real use case demands it
2. Someone volunteers 200 lines
3. Test coverage is comprehensive first
