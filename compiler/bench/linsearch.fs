\ expected: 49950000
\ Linear search benchmark - search 100K times in unsorted array

1000 constant SIZE
100000 constant ITERS
create arr SIZE cells allot

: arr@ ( i -- n ) cells arr + @ ;
: arr! ( n i -- ) cells arr + ! ;

: lsearch ( val -- idx|-1 )
  SIZE 0 do
    dup i arr@ = if drop i unloop exit then
  loop
  drop -1 ;

: init-arr ( -- )
  \ Shuffle pattern: store (i*7) mod SIZE at index i
  SIZE 0 do
    i 7 * SIZE mod i arr!
  loop ;

: bench ( -- sum )
  0
  ITERS 0 do
    i SIZE mod lsearch +
  loop ;

: main
  init-arr
  bench . cr ;
