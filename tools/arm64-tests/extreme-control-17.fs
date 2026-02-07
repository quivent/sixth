\ expect: 21
\ Test: BEGIN-UNTIL with condition that consumes flag correctly
\ UNTIL exits when flag is TRUE (non-zero)

: sum-to ( n -- sum )
  \ Sum 1 to n using UNTIL
  0 swap       \ ( sum counter )
  begin
    dup rot    \ ( counter counter sum )
    +          \ ( counter sum+counter )
    swap       \ ( sum counter )
    1-         \ ( sum counter-1 )
    dup 0=     \ ( sum counter flag: counter=0? )
  until
  drop         \ drop counter
;

: main
  6 sum-to     \ 6+5+4+3+2+1 = 21
;
