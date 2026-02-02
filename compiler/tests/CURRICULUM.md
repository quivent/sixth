# Sixth Compiler Test Curriculum

A complete Forth compiler passes all of these. No exceptions.

---

## 1. STACK OPERATIONS

The stack is everything. If these fail, nothing works.

### 1.1 Basic Manipulators
- `dup` - duplicate TOS
- `drop` - discard TOS
- `swap` - exchange top two
- `over` - copy second to top
- `rot` - rotate top three (a b c -- b c a)
- `-rot` - reverse rotate (a b c -- c a b)
- `nip` - drop second (a b -- b)
- `tuck` - copy TOS below second (a b -- b a b)

### 1.2 Deep Stack
- `pick` - copy nth item (0 pick = dup)
- `roll` - rotate nth item to top
- `2dup` - duplicate top pair
- `2drop` - drop top pair
- `2swap` - exchange top two pairs
- `2over` - copy second pair
- `depth` - current stack depth

### 1.3 Edge Cases
- depth after operations matches expected
- pick with n=0, n=1, n=large
- operations on minimum stack (1 or 2 items)
- stack underflow behavior

---

## 2. ARITHMETIC

### 2.1 Basic Operations
- `+` `-` `*` `/` `mod` `/mod`
- `1+` `1-` `2+` `2-` `2*` `2/`
- `negate` `abs`

### 2.2 Signed Edge Cases
- MAX-INT + 1 (overflow → MIN-INT)
- MIN-INT - 1 (underflow → MAX-INT)
- MIN-INT negate (special: result is still MIN-INT)
- MIN-INT -1 / (undefined on many systems - may trap)
- Division by zero behavior

### 2.3 Division Semantics (CRITICAL)
Forth allows floored or symmetric division. Test which you have:
- `-7 3 /` = -3 (symmetric) or -2 (floored)?
- `-7 3 mod` = 2 (symmetric) or -1 (floored)?
- `-7 -3 /` = 2 (both)
- `-7 -3 mod` = -1 (symmetric) or 2 (floored)?
- `7 -3 /` = -2 (symmetric) or -3 (floored)?
- `7 -3 mod` = 1 (symmetric) or -2 (floored)?

The tests must know which semantics to expect.

### 2.4 Unsigned Operations
- `u<` `u>`
- `um*` `um/mod` (if supported)
- Large unsigned values near MAX-UINT
- `-1` treated as MAX-UINT in unsigned ops

### 2.5 Operand Order (CRITICAL)
Non-commutative operations: second-on-stack OP top-of-stack
- `10 3 -` = 10 - 3 = 7 (NOT 3 - 10)
- `10 3 /` = 10 / 3 = 3 (NOT 3 / 10)
- `10 3 mod` = 10 mod 3 = 1
- `5 2 swap -` = 2 - 5 = -3

Test both orders to catch confusion.

### 2.6 Mixed Operations
- Chains: `1 2 + 3 * 4 -` = (1+2)*3 - 4 = 5
- Self operations: `5 dup *` = 25, `10 dup -` = 0
- With negatives: `-5 3 +` = -2, `5 -3 *` = -15

---

## 3. COMPARISON AND LOGIC

### 3.1 Comparisons
- `=` `<>` `<` `>` `<=` `>=`
- `0=` `0<` `0>` `0<>`
- `u<` `u>`

### 3.2 Comparison Edge Cases
- 0 0 = (equal zeros)
- -1 0 < (negative vs zero)
- MAX-INT MIN-INT > (extremes)
- Signed vs unsigned interpretation of large values

### 3.3 Bitwise Logic
- `and` `or` `xor` `invert`
- 0 invert = -1 (all bits set)
- -1 0 and = 0
- Masks: `$FF and` extracts low byte

### 3.4 Boolean Logic
- Forth true = -1, false = 0
- Any non-zero is truthy in conditionals
- `1 if` should execute then-branch

---

## 4. MEMORY OPERATIONS

### 4.1 Cell Access
- `!` `@` - store and fetch
- `+!` - add to memory
- `?` - fetch and print (convenience)

### 4.2 Byte Access
- `c!` `c@` - character/byte operations
- Byte ordering on multi-byte values

### 4.3 Variables and Constants
- `variable` creates cell storage
- `constant` creates immutable value
- `value` and `to` (if supported)
- Multiple variables don't alias
- Constants in expressions

### 4.4 Memory Edge Cases
- Writing then reading same location
- Adjacent variables are separate
- Alignment requirements

---

## 5. CONTROL FLOW: CONDITIONALS

### 5.1 IF-THEN
- `condition if ... then`
- Executes body when true (non-zero)
- Skips body when false (zero)

### 5.2 IF-ELSE-THEN
- `condition if ... else ... then`
- Mutually exclusive branches
- Both branches can have complex code

### 5.3 Nested Conditionals
- `if if ... then then`
- `if ... else if ... then then`
- Deep nesting (3+ levels)

### 5.4 Conditional Edge Cases
- 1 as true
- -1 as true
- Large positive as true
- Large negative as true
- Only 0 is false
- Stack preserved across branches
- Stack effects must match in both branches

---

## 6. CONTROL FLOW: LOOPS

### 6.1 DO-LOOP
- `limit start do ... loop`
- `i` returns current index
- `j` returns outer loop index (nested)
- Executes (limit - start) times
- `10 0 do` runs 10 times (0-9)

### 6.2 DO-+LOOP
- `limit start do ... step +loop`
- Custom increment
- Negative step for countdown
- Terminates when crossing limit

### 6.3 BEGIN-UNTIL
- `begin ... condition until`
- Executes at least once
- Repeats while condition is false
- Exits when condition is true

### 6.4 BEGIN-WHILE-REPEAT
- `begin ... condition while ... repeat`
- Tests before body
- May execute zero times
- `begin condition while body repeat`

### 6.5 BEGIN-AGAIN
- Infinite loop
- Must use `exit` to escape (NOT leave - leave is for do-loops only)

### 6.6 LEAVE
- Exits innermost DO-LOOP only (not begin loops)
- Control goes to after `loop` or `+loop`
- Must clean up return stack properly

### 6.7 UNLOOP
- Removes loop parameters from return stack
- Required before `exit` inside do-loop
- `10 0 do i 5 = if unloop exit then loop`

### 6.8 ?DO (optional)
- Like DO but executes zero times if limit = start
- `5 5 ?do i . loop` prints nothing
- Standard DO may or may not execute with equal bounds (implementation defined)

### 6.9 Loop Edge Cases
- Empty loop body: `10 0 do loop`
- Single iteration: `1 0 do i . loop`
- Equal bounds: `5 5 do i . loop` (behavior varies - test your implementation)
- Negative step: `0 10 do i . -1 +loop` prints 10 9 8 7 6 5 4 3 2 1 (starts at 10, stops before 0)
- Nested loops with both i and j
- Loop with conditionals inside
- Multiple leaves in same loop
- Leave inside nested if
- Exit inside do-loop (requires unloop first)
- +loop that overshoots: `10 0 do i . 3 +loop` prints 0 3 6 9
- +loop exact hit: `9 0 do i . 3 +loop` prints 0 3 6 (9 is limit, not printed)
- +loop step = 0: infinite loop (test with timeout/limit)
- Loop body modifies nothing: `10 0 do loop` should complete in ~0 time
- Counted iterations: `0 10 0 do 1+ loop .` prints 10

---

## 7. RETURN STACK

### 7.1 Basic Operations
- `>r` - move to return stack
- `r>` - move from return stack
- `r@` - copy from return stack

### 7.2 Double Operations
- `2>r` `2r>` `2r@`

### 7.3 Return Stack Discipline
- Must be balanced within a word
- Cannot hold values across word boundaries (undefined)
- Using with loops (i uses return stack)

### 7.4 Edge Cases
- `>r ... r>` preserving a value
- Multiple values: `>r >r ... r> r>` (order matters)
- Interaction with do-loop indices

---

## 8. WORDS AND DEFINITIONS

### 8.1 Word Definition
- `: name ... ;`
- Defining and calling words
- Words calling other words
- Forward reference (if supported)

### 8.2 Recursion
- `recurse` - self-call within definition
- Base case required
- Stack passing through recursion
- Deep recursion (100+ levels)

### 8.3 EXIT
- `exit` - early return from word
- Value on stack at exit is return value
- Multiple exit points

### 8.4 Edge Cases
- Empty word: `: nop ;`
- Single operation word
- Long words (many operations)
- Deeply nested calls (a calls b calls c calls d...)

---

## 9. STRINGS AND OUTPUT

### 9.1 Numeric Output
- `.` - print signed number
- `u.` - print unsigned
- `.r` - right-justified in field (if supported)

### 9.2 Character Output
- `emit` - output character
- `cr` - newline
- `space` `spaces`
- `bl` - blank character constant (32)

### 9.3 String Literals
- `s" string"` - leaves addr len on stack
- `." string"` - prints immediately
- `c" string"` - counted string (if supported)

### 9.4 String Edge Cases
- Empty string: `s" "`
- String with spaces
- Multiple strings in one word
- String spanning... no, strings don't span lines in standard Forth

---

## 10. SPECIAL WORDS

### 10.1 Stack Queries
- `depth` - number of items
- `.s` - print stack non-destructively

### 10.2 Arithmetic Extras
- `min` `max`
- `within` - range test: `( test lo hi -- flag )`
  - ANS semantics: true if lo <= test < hi using UNSIGNED comparison
  - This allows wrap-around ranges
  - `5 0 10 within` = true
  - `5 5 10 within` = true (at low boundary)
  - `10 5 10 within` = false (at high boundary)
  - `-1 0 10 within` = false (unsigned: -1 is MAX-UINT)
- `*/` `*/mod` - scaled arithmetic (intermediate double-width)

### 10.3 Counted Strings
- `count` - `( c-addr -- c-addr+1 u )` get length byte

### 10.4 Bit Operations
- `lshift` `rshift`
- `2*` = 1 lshift
- `2/` = 1 rshift (arithmetic, preserves sign)

### 10.5 Shift Edge Cases
- `1 0 lshift` = 1 (shift by 0)
- `1 63 lshift` = $8000000000000000 (MIN-INT as unsigned)
- `1 64 lshift` = ? (undefined or 0 on x86-64)
- `-1 1 rshift` - does it sign-extend or zero-fill?
- `$8000000000000000 1 rshift` - sign preservation?

---

## 11. INTERACTION PATTERNS

These test features working together.

### 11.1 Loop + Conditional
- `do i 2 mod 0= if i . then loop` (print evens)
- `begin ... if ... exit then ... until`

### 11.2 Loop + Return Stack
- Save value across loop iterations
- Use r@ to read without consuming

### 11.3 Recursion + Loop
- Recursive word containing a loop
- Loop calling recursive word

### 11.4 Multiple Words
- Word A calls B, B calls C
- A and B both modify stack
- Mutual recursion (if possible)

### 11.5 Memory + Loop
- Fill array with loop
- Sum array with loop
- Search array with early exit

---

## 12. LITERAL EXPRESSIONS

Test that expressions with only literals produce correct results.

### 12.1 Literal Arithmetic
- `: test 3 4 + . ;` → prints 7
- `: test 10 2 / . ;` → prints 5
- `: test 2 3 + 4 * . ;` → prints 20

Whether compiler folds or not, result must be correct.

### 12.2 Literal Conditionals
- `: test 0 if 1 else 2 then . ;` → prints 2
- `: test -1 if 1 else 2 then . ;` → prints 1
- `: test 1 if 1 else 2 then . ;` → prints 1 (any non-zero is true)

### 12.3 Literal Edge Cases
- Overflow in literals: does `$7FFFFFFFFFFFFFFF 1 +` in source wrap correctly?
- Large literal: `9223372036854775807` parses correctly?

---

## 13. STRESS TESTS

### 13.1 Deep Stack
- 20+ values on stack simultaneously
- Operations that touch deep items
- Verify all values survive

### 13.2 Many Live Values
```forth
: stress
  1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16
  + + + + + + + + + + + + + + + ;
\ = 136
```

### 13.3 Deep Nesting
- 5+ nested if/then
- 3+ nested do-loops
- Recursion 200+ deep

---

## 14. EDGE CASES THAT BREAK COMPILERS

### 14.1 Stack Depth Changes in Branches
```forth
: bad  flag if 1 2 then ;  \ ILLEGAL - different depths
```
Both branches must leave stack in same state.

### 14.2 Return Stack Inside Loop
```forth
: risky  10 0 do  i >r  r> . loop ;
```
Does `>r` inside a do-loop interfere with loop operation?
Test it. Don't assume. Result defines the implementation.

### 14.3 Boundary Values (64-bit)
Actual values, not abstractions:
- `0` `1` `-1`
- `$7FFFFFFFFFFFFFFF` (MAX-INT = 9223372036854775807)
- `$8000000000000000` (MIN-INT = -9223372036854775808)
- `$7FFFFFFFFFFFFFFF 1 +` → wraps to MIN-INT
- `$8000000000000000 1 -` → wraps to MAX-INT
- `$8000000000000000 negate` → still MIN-INT (no positive representation)

### 14.4 Bit Patterns That Expose Bugs
- `$01` vs `$80` - opposite ends of byte
- `$00FF` vs `$FF00` - byte swap detection
- `$5555555555555555` vs `$AAAAAAAAAAAAAAAA` - alternating bits
- `$0F0F0F0F0F0F0F0F` vs `$F0F0F0F0F0F0F0F0` - nibble swap

### 14.4 Empty Constructs
- Empty if: `flag if then`
- Empty loop: `10 0 do loop`
- Empty else: `flag if 1 else then` - is this legal?

### 14.5 Self-Reference
- `5 dup dup * *` (5^3)
- `1 2 3 rot rot rot` (back to original)
- Operations that should be no-ops

---

## REVIEW CHECKLIST

For each category:
1. Does every word have at least one test?
2. Does every word have an edge case test?
3. Are interactions between features tested?
4. Are error conditions tested (where defined)?
5. Do tests verify stack effects?

---

## FIRST REVIEW NOTES

Missing from above:
- `?dup` - duplicate if non-zero: `0 ?dup` = `0`, `5 ?dup` = `5 5`
- `cell+` `cells` - address arithmetic (cell = 8 bytes on 64-bit)
- `chars` `char+` - character address arithmetic
- `aligned` `align` - alignment
- `allot` - reserve space in dictionary
- `here` - current dictionary/data pointer
- `['] name` - get execution token at compile time
- `execute` - run word from execution token on stack
- `create ... does>` - defining words (metaprogramming)
- Immediate words and compilation semantics
- `[` `]` - switch interpretation mode in colon definition
- `literal` - compile number into definition
- `postpone` - compile compilation semantics
- `state` - compilation state flag
- `[char] x` - compile character code

These are advanced. Core tests come first.

### Missing Comparison Edge Cases
- Comparing same value: `5 5 =` `5 5 <` `5 5 >`
- Comparing with zero: `0 0=` `-1 0=` `1 0=`
- `u<` with negative numbers: `-1 1 u<` is FALSE (unsigned -1 is huge)

---

## THIRD REVIEW: NUMBER PARSING

### Decimal
- Positive: `42` `100` `1`
- Zero: `0`
- Negative: `-1` `-42` `-100`

### Hexadecimal
- `$FF` = 255
- `$FFFFFFFF` = 4294967295 (32-bit max)
- `$FFFFFFFFFFFFFFFF` = -1 (64-bit all ones)
- Lowercase: `$ff` `$deadbeef`

### Verify Parsing
- `-5` parsed = `5 negate` computed
- Large numbers near MAX-INT
- Numbers with leading zeros: `007` = 7

---

## SECOND REVIEW NOTES

Patterns that catch bugs:

1. **Off-by-one**: Loop runs n vs n-1 vs n+1 times
2. **Sign extension**: Bytes loaded as signed or unsigned
3. **Comparison direction**: `<` vs `>` confusion
4. **Stack order**: Which operand is TOS for non-commutative ops
5. **Branch offset**: Jump targets calculated wrong
6. **Register clobber**: Value destroyed by operation
7. **Alignment**: Unaligned access on strict architectures

Each pattern needs tests designed to expose it.

---

## PRIORITY ORDER

1. Stack operations (everything depends on these)
2. Basic arithmetic
3. Comparisons
4. Simple conditionals
5. Simple loops
6. Memory operations
7. Word definitions
8. Complex loops (nested, +loop)
9. Return stack
10. Strings
11. Interactions
12. Edge cases
13. Optimizations

Test the foundation first. Build up.

---

## 15. BUG-CATCHING PATTERNS

Specific tests designed to catch common implementation bugs.

### 15.1 Off-by-One
- `10 0 do loop` - exactly 10 iterations, not 9 or 11
- `depth` after `1 2 3` = 3, not 2 or 4
- `pick` indexing: `0 pick` = dup, `1 pick` = over

### 15.2 Sign Errors
- `c@` must zero-extend, not sign-extend: `$FF c@` = 255, not -1
- `0 1 -` = -1
- `-1 abs` = 1

### 15.3 Operand Swap
For every non-commutative operation, test both:
- `a b op` and verify against known result
- Different values so swap is detectable: `7 3 -` = 4, not -4

### 15.4 Branch Target Errors
- Nested if: inner then jumps to right place
- Else after long then-body
- Loop with long body
- Forward jumps across many bytes

### 15.5 Stack Corruption
- Value survives operation it's not part of: `1 2 3 drop` leaves `1 2`
- Value survives loop: `99 10 0 do drop i loop` - 99 consumed, leaves 9
- Value survives call: `42 helper` where helper uses stack then restores

### 15.6 Register Pressure
Test with many live values:
```forth
: pressure  1 2 3 4 5 6 7 8 + + + + + + + ;  \ = 36
```
All values must survive until used.

### 15.7 Return Stack Balance
```forth
: balanced  >r 1 2 + r> + ;  \ 5 balanced = 8
```
Return stack must be clean at word exit.

### 15.8 Loop Index Integrity
```forth
: nested  3 0 do 3 0 do i j * . loop loop ;
```
Inner i and outer j must not interfere.

---

## 16. FORBIDDEN PATTERNS

Tests for things that MUST fail or have defined behavior.

### 16.1 Stack Underflow
- `drop` with empty stack
- `swap` with one item
- `+` with one item

### 16.2 Division by Zero
- `5 0 /`
- `5 0 mod`

### 16.3 Unbalanced Control
- `if` without `then`
- `do` without `loop`
- `begin` without `until` or `again`
- These should be compile-time errors

### 16.4 Unbalanced Return Stack
- `>r` without matching `r>`
- `r>` without prior `>r`

---

## 17. STACK EFFECT VERIFICATION

Every test should implicitly verify stack effects.

### Pattern
```forth
\ expect: 42
: main  1 2 3 drop drop 42 . ;
```
If stack is wrong, output will be wrong or crash will occur.

### Explicit Depth Checks
```forth
: verify-add  depth 2 = if + . else ." stack wrong" then ;
: main  5 3 verify-add ;
```

### Multi-Result Tests
```forth
\ expect: 3 2 1
: main  1 2 3 . . . ;
```
Output is LIFO: 3 printed first, then 2, then 1. Order verifies stack.

---

## 18. ALGORITHMIC TESTS

These exercise multiple features together. They catch interaction bugs.

### 18.1 Factorial
```forth
: fact ( n -- n! )  dup 1 > if dup 1- recurse * else drop 1 then ;
: main  5 fact . ;
\ expect: 120
```
Tests: recursion, comparison, arithmetic, conditionals

### 18.2 Fibonacci
```forth
: fib ( n -- fib[n] )
  dup 2 < if exit then
  dup 1- recurse swap 2 - recurse + ;
: main  10 fib . ;
\ expect: 55
```
Tests: recursion, stack manipulation, arithmetic

### 18.3 GCD
```forth
: gcd ( a b -- gcd )  ?dup if tuck mod recurse then ;
: main  48 18 gcd . ;
\ expect: 6
```
Tests: ?dup, tuck, mod, recursion

### 18.4 Prime Test
```forth
: prime? ( n -- flag )
  dup 2 < if drop 0 exit then
  dup 2 = if drop -1 exit then
  dup 2 mod 0= if drop 0 exit then
  3 begin 2dup dup * >= while
    2dup mod 0= if 2drop 0 exit then
    2 +
  repeat 2drop -1 ;
: main  17 prime? . 18 prime? . ;
\ expect: -1 0
```
Tests: multiple exits, while loop, arithmetic, comparison

### 18.5 Array Sum
```forth
variable arr  10 cells allot
: init  10 0 do i arr i cells + ! loop ;
: sum   0  10 0 do arr i cells + @ + loop ;
: main  init sum . ;
\ expect: 45
```
Tests: variables, allot, do-loop, i, memory access, cells arithmetic

### 18.6 Bubble Sort (stress test)
```forth
variable data  5 cells allot
: !data  5 data ! 3 data cell+ ! 8 data 2 cells + ! 1 data 3 cells + ! 9 data 4 cells + ! ;
: @i ( i -- val )  cells data + @ ;
: !i ( val i -- )  cells data + ! ;
: sort
  4 0 do
    4 i - 0 do
      i @i i 1+ @i > if
        i @i i 1+ @i  i !i i 1+ !i
      then
    loop
  loop ;
: show  5 0 do i @i . loop ;
: main  !data sort show ;
\ expect: 1 3 5 8 9
```
Tests: everything

---

## SUMMARY

A test suite following this curriculum will catch:
1. Basic correctness errors
2. Off-by-one bugs
3. Operand order confusion
4. Stack tracking errors
5. Control flow bugs
6. Register allocation failures
7. Interaction bugs between features

The compiler comes to these tests blind. If it passes, it works. If it fails, find the bug.
