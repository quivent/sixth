\ expect: 1
\ ADVERSARIAL: Test boundary lengths 1-10
\ Loop should work correctly for all small lengths

: test-len ( n -- flag )
  \ Build path of length n
  s" /tmp/xxxxx" drop swap   \ ( addr n )
  0 open-file drop           \ open with truncated length
  drop                       \ discard fd
  1                          \ didn't crash
;

: main
  1 test-len 0= if 0 exit then
  2 test-len 0= if 0 exit then
  3 test-len 0= if 0 exit then
  4 test-len 0= if 0 exit then
  5 test-len 0= if 0 exit then
  6 test-len 0= if 0 exit then
  7 test-len 0= if 0 exit then
  8 test-len 0= if 0 exit then
  9 test-len 0= if 0 exit then
  10 test-len 0= if 0 exit then
  1
;
