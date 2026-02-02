\ expected: 9592

: isqrt ( n -- sqrt )
  dup 2 < if exit then
  dup begin 2dup / over + 2/ 2dup > while nip repeat
  drop nip ;

: is-prime ( n -- flag )
  dup 2 < if drop 0 exit then
  dup 2 = if drop 1 exit then
  dup 2 mod 0= if drop 0 exit then
  dup isqrt 3
  begin 2dup >= while
    2 pick over mod 0= if 2drop drop 0 exit then
    2 +
  repeat 2drop drop 1 ;

: main 0 100000 2 do i is-prime if 1+ then loop . cr ;
