\ Adversarial Test 0003: Comparison Operations
\ Every boundary. Every sign. Every edge.

: test-equal-true ( -- )
  5 5 = if ." PASS" else ." FAIL" then cr ;

: test-equal-false ( -- )
  5 6 = if ." FAIL" else ." PASS" then cr ;

: test-not-equal-true ( -- )
  5 6 <> if ." PASS" else ." FAIL" then cr ;

: test-not-equal-false ( -- )
  5 5 <> if ." FAIL" else ." PASS" then cr ;

: test-less-than-true ( -- )
  3 5 < if ." PASS" else ." FAIL" then cr ;

: test-less-than-false ( -- )
  5 3 < if ." FAIL" else ." PASS" then cr ;

: test-less-than-equal ( -- )
  5 5 < if ." FAIL" else ." PASS" then cr ;

: test-greater-than-true ( -- )
  5 3 > if ." PASS" else ." FAIL" then cr ;

: test-greater-than-false ( -- )
  3 5 > if ." FAIL" else ." PASS" then cr ;

: test-less-negative ( -- )
  -5 -3 < if ." PASS" else ." FAIL" then cr ;

: test-greater-negative ( -- )
  -3 -5 > if ." PASS" else ." FAIL" then cr ;

: test-zero-less ( -- )
  -1 0< if ." PASS" else ." FAIL" then cr ;

: test-zero-less-false ( -- )
  1 0< if ." FAIL" else ." PASS" then cr ;

: test-zero-less-zero ( -- )
  0 0< if ." FAIL" else ." PASS" then cr ;

: test-zero-equal ( -- )
  0 0= if ." PASS" else ." FAIL" then cr ;

: test-zero-equal-false ( -- )
  1 0= if ." FAIL" else ." PASS" then cr ;

." 0003-comparison-torture: " cr
." 5=5:     " test-equal-true
." 5=6:     " test-equal-false
." 5<>6:    " test-not-equal-true
." 5<>5:    " test-not-equal-false
." 3<5:     " test-less-than-true
." 5<3:     " test-less-than-false
." 5<5:     " test-less-than-equal
." 5>3:     " test-greater-than-true
." 3>5:     " test-greater-than-false
." -5<-3:   " test-less-negative
." -3>-5:   " test-greater-negative
." -1 0<:   " test-zero-less
." 1 0<:    " test-zero-less-false
." 0 0<:    " test-zero-less-zero
." 0 0=:    " test-zero-equal
." 1 0=:    " test-zero-equal-false

\ REMINDER: Compiler-agnostic tests. Pure Forth semantics.
