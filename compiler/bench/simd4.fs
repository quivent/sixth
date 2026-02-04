\ expected: 3334
\ Count matching elements - vectorizable conditional count

create arr 80000 allot

: init ( -- )
  10000 0 do
    i 3 mod i 8 * arr + !
  loop ;

: count-zeros ( -- n )
  0 10000 0 do
    i 8 * arr + @ 0= if 1+ then
  loop ;

: main init count-zeros . cr ;
