\ adversarial/0023-j-outer-index.fs - Test j word (outer loop index in nested DO loops)
\ j returns the index of the outer DO loop when inside a nested DO loop

\ Test 1: Basic nested loop - j is outer, i is inner
\ Outer: 0 to 2, Inner: 0 to 2
\ Should print pairs: (j=0,i=0) (j=0,i=1) (j=0,i=2) (j=1,i=0) ...
: test-basic-ji ( -- )
  3 0 do          \ outer loop: j = 0, 1, 2
    3 0 do        \ inner loop: i = 0, 1, 2
      j . i .     \ print outer then inner index
    loop
  loop
;

\ Expected: 0 0 0 1 0 2 1 0 1 1 1 2 2 0 2 1 2 2
test-basic-ji cr

\ Test 2: Sum using both i and j
\ Sum of (i + j) for i=0..2, j=0..2 should be:
\ (0+0)+(0+1)+(0+2) + (1+0)+(1+1)+(1+2) + (2+0)+(2+1)+(2+2)
\ = 0+1+2 + 1+2+3 + 2+3+4 = 3 + 6 + 9 = 18
variable sum-ij
: test-sum-ij ( -- n )
  0 sum-ij !
  3 0 do
    3 0 do
      i j + sum-ij +!
    loop
  loop
  sum-ij @
;

test-sum-ij . cr  \ Expected: 18

\ Test 3: Product pattern - j*10 + i gives unique values
\ Should produce: 0 1 2 10 11 12 20 21 22
: test-product-pattern ( -- )
  3 0 do
    3 0 do
      j 10 * i + .
    loop
  loop
;

test-product-pattern cr

\ Test 4: Different loop bounds - outer 10-13, inner 0-2
\ j should be 10, 11, 12 while i is 0, 1, 2
: test-different-bounds ( -- )
  13 10 do       \ j = 10, 11, 12
    3 0 do       \ i = 0, 1, 2
      j . i .
    loop
  loop
;

test-different-bounds cr  \ Expected: 10 0 10 1 10 2 11 0 11 1 11 2 12 0 12 1 12 2

\ Test 5: Verify j stays constant during inner loop iteration
\ For each outer iteration, j should remain the same
variable last-j
variable j-changed
: test-j-constant ( -- )
  0 j-changed !
  3 0 do
    j last-j !           \ store j at start of inner loop
    5 0 do
      j last-j @ <> if
        1 j-changed !    \ flag if j ever changed during inner
      then
    loop
  loop
  j-changed @ if
    ." FAIL: j changed during inner loop" cr
  else
    ." PASS: j constant during inner loop" cr
  then
;

test-j-constant

\ Test 6: Accumulate j values - should get j added once per inner iteration
\ Inner runs 4 times per outer, outer is 0,1,2
\ Sum of j = 0*4 + 1*4 + 2*4 = 0 + 4 + 8 = 12
variable j-sum
: test-j-accumulate ( -- n )
  0 j-sum !
  3 0 do
    4 0 do
      j j-sum +!
    loop
  loop
  j-sum @
;

test-j-accumulate . cr  \ Expected: 12

\ Test 7: Difference between j and i
\ When outer=2, inner=0: j-i = 2
\ When outer=0, inner=2: j-i = -2
: test-j-minus-i ( -- )
  3 0 do
    3 0 do
      j i - .
    loop
  loop
;

test-j-minus-i cr  \ Expected: 0 -1 -2 1 0 -1 2 1 0

\ Test 8: Use j in conditional inside inner loop
\ Count how many times j > i
variable ji-count
: test-j-greater-i ( -- n )
  0 ji-count !
  4 0 do
    4 0 do
      j i > if
        1 ji-count +!
      then
    loop
  loop
  ji-count @
;

test-j-greater-i . cr  \ Expected: 6 (pairs where j>i: 10,20,21,30,31,32)

\ Test 9: Triple nesting conceptual test
\ In triple nesting, j gives second-level (middle) index
\ Outermost is accessed via return stack manipulation (not standard)
\ Here we just verify j works in the middle position
: test-triple-nest ( -- )
  2 0 do              \ outermost: k conceptually
    2 0 do            \ middle: this is what j accesses
      2 0 do          \ innermost: this is what i accesses
        j . i .       \ j=middle index, i=inner index
      loop
    loop
  loop
;

." Triple nested (j=middle, i=inner): "
test-triple-nest cr

\ Test 10: Negative indices with j
: test-negative-j ( -- )
  2 -2 do           \ j = -2, -1, 0, 1
    2 0 do          \ i = 0, 1
      j . i .
    loop
  loop
;

test-negative-j cr  \ Expected: -2 0 -2 1 -1 0 -1 1 0 0 0 1 1 0 1 1

\ Test 11: Single iteration outer loop - j should be start value
: test-single-outer ( -- )
  43 42 do          \ j = 42 only
    3 0 do
      j .
    loop
  loop
;

test-single-outer cr  \ Expected: 42 42 42

\ Test 12: Verify i and j are independent
\ Change nothing about outer loop, inner should still work
variable outer-sum
variable inner-sum
: test-independence ( -- )
  0 outer-sum !
  0 inner-sum !
  5 0 do
    j outer-sum +!
    3 0 do
      i inner-sum +!
    loop
  loop
  ." Outer sum (j): " outer-sum @ . cr   \ 0+1+2+3+4 = 10
  ." Inner sum (i): " inner-sum @ . cr   \ (0+1+2)*5 = 15
;

test-independence

\ REMINDER: j is the outer loop index. i is inner. Get them backwards, subtle bugs.
