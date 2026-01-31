\ expect: 0 1 1 0 1
\ Primality check
: prime? ( n -- 0|1 )
  dup 2 < if drop 0 exit then
  dup 2 = if drop 1 exit then
  dup 1 and 0= if drop 0 exit then
  3
  begin
    2dup dup * >= while    \ while i*i <= n
    2dup mod 0= if drop drop 0 exit then
    2+
  repeat
  drop drop 1 ;
: main
  1 prime? .
  7 prime? .
  23 prime? .
  15 prime? .
  97 prime? .
  cr ;
