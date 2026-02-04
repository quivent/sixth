\ expected: 49950000
\ Counting sort benchmark - sort 100K elements

100000 constant SIZE
1000 constant RANGE
create arr SIZE cells allot
create cnt RANGE cells allot
create out SIZE cells allot

: arr@ ( i -- n ) cells arr + @ ;
: arr! ( n i -- ) cells arr + ! ;
: cnt@ ( i -- n ) cells cnt + @ ;
: cnt! ( n i -- ) cells cnt + ! ;
: out@ ( i -- n ) cells out + @ ;
: out! ( n i -- ) cells out + ! ;

: counting-sort ( -- )
  \ Clear counts
  RANGE 0 do 0 i cnt! loop
  \ Count occurrences
  SIZE 0 do
    i arr@ dup cnt@ 1+ swap cnt!
  loop
  \ Cumulative count (descending order)
  RANGE 2 - 0 swap do
    i cnt@ i 1+ cnt@ + i cnt!
  -1 +loop
  \ Build output
  SIZE 0 do
    i arr@                \ val
    dup cnt@ 1-           \ val idx
    dup rot cnt!          \ idx ; update count
    i arr@ swap out!      \ store val at out[idx]
  loop
  \ Copy back
  SIZE 0 do i out@ i arr! loop ;

: init-arr ( -- )
  SIZE 0 do
    i RANGE mod i arr!
  loop ;

: sum-arr ( -- n )
  0 SIZE 0 do i arr@ + loop ;

: main
  init-arr
  counting-sort
  sum-arr . cr ;
