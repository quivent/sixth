# Optimization: Deep Recursion Unrolling

## Problem

Double recursion like Ackermann:
```forth
: ack ( m n -- r )
  over 0= if nip 1+ exit then
  dup 0= if drop 1- 1 recurse exit then
  over 1- -rot 1- recurse recurse ;
```

Each non-tail `recurse` costs:
- Push args to data stack
- Push return to return stack
- Call
- Pop return
- Pop results

For ack(3,10): ~3 million calls. Sixth: 53ms. GCC -O2: 12ms. GCC is 4.4x faster.

## What GCC Does

GCC keeps multiple recursion frames in registers:
```
rbx  = m1    r12 = m2    r13 = m3    r14 = m4
rax  = n1    ...
```

Loop instead of call. Only real call when depth > 4.

## Solution: `recursive` Declaration

Require explicit declaration. Compiler enforces and optimizes.

```forth
: ack ( m n -- r ) recursive
  over 0= if nip 1+ exit then
  dup 0= if drop 1- 1 recurse exit then
  over 1- -rot 1- recurse recurse ;
```

### Safety: Catches Errors

```forth
: foo ( n -- n )
  dup 0= if exit then
  1- recurse ;          \ ERROR: recurse without recursive declaration
```

```
ABORT: 'recurse' used but word not declared 'recursive'
```

Programmer cannot accidentally write deep recursion. Must declare intent.

### Optimization: Enables Cache

Since programmer declared `recursive`, compiler reserves r12-r14 for this word only. No global calling convention change.

- Non-recursive words: unchanged, no overhead
- Recursive words: cache loop, 2-3x faster on deep recursion

### Implementation

```forth
variable is-recursive  0 is-recursive !
variable saw-recurse   0 saw-recurse !

: recursive ( -- )
  1 is-recursive ! ;

\ In compile-builtin, when $recurse encountered:
  1 saw-recurse !
  is-recursive @ 0= if
    s" recurse without recursive declaration" abort
  then
  is-recursive @ 1 > if
    \ generate cache-loop recurse
  else
    \ generate normal recurse
  then

\ In end-def:
  saw-recurse @ is-recursive @ 0= and if
    s" recurse without recursive declaration" abort
  then
  is-recursive @ if
    \ generate cache-loop epilogue
  then
  0 is-recursive !
  0 saw-recurse !
```

### Register Cache

Use r12-r14 for recursion cache (3 frames). Only in `recursive` words.

```
r12 = cached frame 0 (m, n packed or split)
r13 = cached frame 1
r14 = cached frame 2
```

r15 remains data stack pointer. No conflict.

### Generated Code

Without `recursive`:
```forth
: foo ( n -- n ) recursive
  dup 0= if exit then
  1- recurse ;
```
```asm
foo:
    test rax, rax
    jz .done
    dec rax
    jmp foo         ; tail-call, already optimized
.done:
    ret
```

With double recursion:
```forth
: ack ( m n -- r ) recursive
  over 0= if nip 1+ exit then
  dup 0= if drop 1- 1 recurse exit then
  over 1- -rot 1- recurse recurse ;
```
```asm
ack:
    ; base case: m=0
    test rbx, rbx
    jnz .not_base1
    mov rax, rax
    inc rax
    ret
.not_base1:
    ; base case: n=0
    test rax, rax
    jnz .not_base2
    dec rbx
    mov rax, 1
    jmp ack         ; tail call
.not_base2:
    ; double recurse: cache current frame
    mov r12, rbx    ; cache m
    dec rbx         ; m for inner call
    dec rax         ; n-1 for inner call

    ; check cache depth, spill if needed
    ; ... cache management ...

    jmp ack         ; loop, not call
```

### Spill When Cache Full

When r12-r14 all used and need 4th frame:

```asm
    push r12
    push r13
    push r14
    call ack        ; real call
    pop r14
    pop r13
    pop r12
```

Rare. Most iterations stay in registers.

## Estimated Size

- Declaration and error checking: 20 lines
- Cache register management: 40 lines
- Loop codegen for double-recurse: 50 lines
- Spill/fill: 30 lines
- Integration: 20 lines

Total: ~160 lines. Compiler grows from 2600 to 2760 lines (6%).

## Benefits

1. **Safety**: Cannot accidentally recurse without thinking
2. **Performance**: 2-3x faster deep recursion when declared
3. **Zero cost**: Non-recursive words unchanged
4. **Explicit**: Programmer states intent, compiler validates
5. **Teachable**: Forces understanding of recursion cost

## Status

**Phase 1 IMPLEMENTED**: `recursive` declaration required. Compiler aborts on `recurse` without it.

**Phase 2 NOT IMPLEMENTED**: Register cache optimization. Declaration enables future optimization.

## Problem: Backwards Compatibility

The `recursive` requirement forced modification of existing valid code:
- `ack.fs` benchmark (working Ackermann implementation)
- 21 test files using `recurse`

This is backwards. Valid Forth code should not need modification to compile.

### Alternative: Pre-Compiler Lint

A separate tool that warns but does not block:

```bash
sixth-lint ack.fs
# Warning: 'recurse' used without 'recursive' declaration at line 3
# Hint: Add 'recursive' after stack comment for safety and optimization

sixth ack.fs
# Compiles and runs normally
```

**Benefits**:
- Existing code works unchanged
- New code gets guidance toward best practice
- Teaching happens without breaking

**Implementation**:
```forth
\ sixth-lint.fs - pre-compiler checker
: check-word ( addr u -- )
  2dup s" recurse" str= if
    is-recursive @ 0= if
      ." Warning: recurse without recursive at line " line# . cr
    then
  then
  ... ;
```

Separate tool. ~50 lines. Does not modify compiler.

### Decision Pending

Current: Strict enforcement (compiler aborts)
Proposed: Lint tool warns, compiler accepts

Chuck Moore principle: The compiler should compile valid Forth. A separate tool can advise.

## Test Cases Needed

```forth
\ Should compile and optimize
: fact ( n -- n! ) recursive
  dup 1 <= if drop 1 exit then
  dup 1- recurse * ;

\ Should compile and optimize (double recurse)
: ack ( m n -- r ) recursive
  over 0= if nip 1+ exit then
  dup 0= if drop 1- 1 recurse exit then
  over 1- -rot 1- recurse recurse ;

\ Should ERROR
: broken ( n -- n )
  1- recurse ;

\ Should compile (no recurse, declaration ignored)
: normal ( n -- n ) recursive
  1+ ;
```
