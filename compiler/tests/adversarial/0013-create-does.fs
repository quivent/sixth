\ Adversarial Test 0013: CREATE DOES>
\ The ultimate test of Forth understanding.

: constant ( n -- ) create , does> @ ;

5 constant five

: test-constant ( -- )
  five 5 = if ." PASS" else ." FAIL" then cr ;

: array ( n -- ) create cells allot does> swap cells + ;

10 array my-array

: test-array-create ( -- )
  42 3 my-array !
  3 my-array @ 42 = if ." PASS" else ." FAIL" then cr ;

: 2constant ( d -- ) create , , does> dup @ swap cell+ @ ;

1 2 2constant one-two

: test-2constant ( -- )
  one-two 1 = swap 2 = and if ." PASS" else ." FAIL" then cr ;

: field ( offset size -- offset+size ) create over , + does> @ + ;

0
cell field .x
cell field .y
constant point-size

create my-point point-size allot

: test-field ( -- )
  10 my-point .x !
  20 my-point .y !
  my-point .x @ 10 = my-point .y @ 20 = and if ." PASS" else ." FAIL" then cr ;

." 0013-create-does: " cr
." constant:  " test-constant
." array:     " test-array-create
." 2constant: " test-2constant
." field:     " test-field

\ REMINDER: CREATE DOES> is how you extend Forth. Get this right, you can build anything.
