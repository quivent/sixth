\ Adversarial Test 0004: Control Flow (ARM64 adapted)

: test-if-true
  1 if 1 else 0 then
  1 = if ." PASS" else ." FAIL" then cr ;

: test-if-false
  0 if 1 else 0 then
  0 = if ." PASS" else ." FAIL" then cr ;

: test-nested-if
  1 if 1 if 42 else 0 then else 0 then
  42 = if ." PASS" else ." FAIL" then cr ;

: test-begin-until
  0 begin 1 + dup 10 = until
  10 = if ." PASS" else ." FAIL" then cr ;

: test-begin-while-repeat
  0 10 begin dup 0> while dup rot + swap 1 - repeat drop
  55 = if ." PASS" else ." FAIL" then cr ;

: main
  ." 0004-control-flow:" cr
  ." if-true:  " test-if-true
  ." if-false: " test-if-false
  ." nested:   " test-nested-if
  ." until:    " test-begin-until
  ." while:    " test-begin-while-repeat
  0 ;
