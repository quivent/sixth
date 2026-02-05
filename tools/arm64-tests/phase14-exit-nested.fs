\ Phase 14: exit in nested control flow
\ expect: 1

: is-prime ( n -- flag )
  dup 2 < if drop 0 exit then
  dup 2 = if drop 1 exit then
  dup 2 mod 0= if drop 0 exit then
  3 begin
    2dup dup * >= while
    2dup mod 0= if 2drop 0 exit then
    2 +
  repeat
  2drop 1 ;

: main 97 is-prime ;
