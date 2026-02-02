\ Adversarial Test 0007: Logic and Bitwise Operations
\ AND OR XOR INVERT. Bit by bit.

: test-and-true ( -- )
  -1 -1 and -1 = if ." PASS" else ." FAIL" then cr ;

: test-and-false ( -- )
  -1 0 and 0 = if ." PASS" else ." FAIL" then cr ;

: test-and-partial ( -- )
  15 7 and 7 = if ." PASS" else ." FAIL" then cr ;

: test-or-false ( -- )
  0 0 or 0 = if ." PASS" else ." FAIL" then cr ;

: test-or-true ( -- )
  -1 0 or -1 = if ." PASS" else ." FAIL" then cr ;

: test-or-partial ( -- )
  8 7 or 15 = if ." PASS" else ." FAIL" then cr ;

: test-xor-same ( -- )
  5 5 xor 0 = if ." PASS" else ." FAIL" then cr ;

: test-xor-different ( -- )
  15 8 xor 7 = if ." PASS" else ." FAIL" then cr ;

: test-invert ( -- )
  0 invert -1 = if ." PASS" else ." FAIL" then cr ;

: test-lshift ( -- )
  1 4 lshift 16 = if ." PASS" else ." FAIL" then cr ;

: test-rshift ( -- )
  16 4 rshift 1 = if ." PASS" else ." FAIL" then cr ;

: test-2star ( -- )
  5 2* 10 = if ." PASS" else ." FAIL" then cr ;

: test-2slash ( -- )
  10 2/ 5 = if ." PASS" else ." FAIL" then cr ;

." 0007-logic-bitwise: " cr
." -1 and -1: " test-and-true
." -1 and 0:  " test-and-false
." 15 and 7:  " test-and-partial
." 0 or 0:    " test-or-false
." -1 or 0:   " test-or-true
." 8 or 7:    " test-or-partial
." 5 xor 5:   " test-xor-same
." 15 xor 8:  " test-xor-different
." invert 0:  " test-invert
." 1 lshift4: " test-lshift
." 16 rshift4:" test-rshift
." 2*:        " test-2star
." 2/:        " test-2slash

\ REMINDER: Bitwise ops must be exact. No rounding. No approximation.
