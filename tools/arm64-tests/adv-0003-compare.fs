\ Adversarial Test 0003: Comparison Operations (ARM64 adapted)

: test-equal
  5 5 = 0<> if ." PASS" else ." FAIL" then cr ;

: test-not-equal
  5 6 <> 0<> if ." PASS" else ." FAIL" then cr ;

: test-less
  3 5 < 0<> if ." PASS" else ." FAIL" then cr ;

: test-greater
  5 3 > 0<> if ." PASS" else ." FAIL" then cr ;

: test-less-equal
  5 5 <= 0<> if ." PASS" else ." FAIL" then cr ;

: test-greater-equal
  5 5 >= 0<> if ." PASS" else ." FAIL" then cr ;

: test-zero-equal
  0 0= 0<> if ." PASS" else ." FAIL" then cr ;

: test-zero-not-equal
  5 0<> 0<> if ." PASS" else ." FAIL" then cr ;

: test-negative-less
  -5 3 < 0<> if ." PASS" else ." FAIL" then cr ;

: test-negative-greater
  3 -5 > 0<> if ." PASS" else ." FAIL" then cr ;

: main
  ." 0003-comparison:" cr
  ." 5=5:     " test-equal
  ." 5<>6:    " test-not-equal
  ." 3<5:     " test-less
  ." 5>3:     " test-greater
  ." 5<=5:    " test-less-equal
  ." 5>=5:    " test-greater-equal
  ." 0 0=:    " test-zero-equal
  ." 5 0<>:   " test-zero-not-equal
  ." -5<3:    " test-negative-less
  ." 3>-5:    " test-negative-greater
  0 ;
