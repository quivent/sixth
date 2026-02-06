\ Adversarial arithmetic test: Zero as operand in all operations
\ expect: 0
\ Verify: 0+n=n, 0*n=0, n/0=0 (ARM64), n-0=n
: test-zero-add   0 42 + 42 = ;
: test-zero-mul   0 42 * 0 = ;
: test-div-zero   42 0 / 0 = ;  \ ARM64 specific
: test-sub-zero   42 0 - 42 = ;
: test-zero-sub   0 42 - -42 = ;

: main
  test-zero-add  if
  test-zero-mul  if
  test-div-zero  if
  test-sub-zero  if
  test-zero-sub  if 0 else 5 then
  else 4 then else 3 then else 2 then else 1 then ;
