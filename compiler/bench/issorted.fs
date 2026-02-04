\ expected: 5000
\ Check if array is sorted benchmark - 10K iterations

1000 constant SIZE
10000 constant ITERS
create arr SIZE cells allot

: arr@ ( i -- n ) cells arr + @ ;
: arr! ( n i -- ) cells arr + ! ;

: is-sorted? ( -- flag )
  1                       \ assume sorted (descending)
  SIZE 1 do
    i 1- arr@ i arr@ < if
      drop 0 unloop exit
    then
  loop ;

: init-sorted ( -- )
  SIZE 0 do SIZE i - 1- i arr! loop ;

: init-unsorted ( -- )
  SIZE 0 do i 7 * SIZE mod i arr! loop ;

: bench ( -- count )
  0
  ITERS 0 do
    i 2 mod if init-unsorted else init-sorted then
    is-sorted? if 1+ then
  loop ;

: main
  bench . cr ;
