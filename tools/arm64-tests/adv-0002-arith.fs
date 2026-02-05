\ Adversarial Test 0002: Arithmetic Edge Cases (ARM64 adapted)

: test-zero-add
  0 0 + 0 = if ." PASS" else ." FAIL" then cr ;

: test-negative-add
  -5 3 + -2 = if ." PASS" else ." FAIL" then cr ;

: test-negative-sub
  3 -5 - 8 = if ." PASS" else ." FAIL" then cr ;

: test-multiply-negative
  -3 4 * -12 = if ." PASS" else ." FAIL" then cr ;

: test-multiply-both-negative
  -3 -4 * 12 = if ." PASS" else ." FAIL" then cr ;

: test-divide-negative
  -12 4 / -3 = if ." PASS" else ." FAIL" then cr ;

: test-mod-negative
  -7 3 mod -1 = if ." PASS" else ." FAIL" then cr ;

: test-negate
  5 negate -5 = if ." PASS" else ." FAIL" then cr ;

: test-negate-negative
  -5 negate 5 = if ." PASS" else ." FAIL" then cr ;

: test-abs-positive
  5 abs 5 = if ." PASS" else ." FAIL" then cr ;

: test-abs-negative
  -5 abs 5 = if ." PASS" else ." FAIL" then cr ;

: main
  ." 0002-arithmetic-edge:" cr
  ." 0+0:      " test-zero-add
  ." -5+3:     " test-negative-add
  ." 3-(-5):   " test-negative-sub
  ." -3*4:     " test-multiply-negative
  ." -3*-4:    " test-multiply-both-negative
  ." -12/4:    " test-divide-negative
  ." -7 mod 3: " test-mod-negative
  ." negate 5: " test-negate
  ." negate-5: " test-negate-negative
  ." abs 5:    " test-abs-positive
  ." abs -5:   " test-abs-negative
  0 ;
