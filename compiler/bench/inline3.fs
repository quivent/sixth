\ expected: 50000000
\ Mutually calling small functions - GCC inlines both

: even? ( n -- flag ) 2 mod 0= ;
: odd? ( n -- flag ) even? 0= ;

: main
  0 100000000 0 do
    i even? if 1+ then
  loop . cr ;
