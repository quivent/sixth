\ expected: 499500
\ Reverse array benchmark - 10K iterations

1000 constant SIZE
10000 constant ITERS
create arr SIZE cells allot

: arr@ ( i -- n ) cells arr + @ ;
: arr! ( n i -- ) cells arr + ! ;

: swap-arr ( i j -- )
  over arr@ over arr@
  rot arr! swap arr! ;

: reverse-arr ( -- )
  0 SIZE 1-
  begin 2dup < while
    2dup swap-arr
    1 under+ 1-
  repeat
  2drop ;

: init-arr ( -- )
  SIZE 0 do i i arr! loop ;

: sum-arr ( -- n )
  0 SIZE 0 do i arr@ + loop ;

: bench ( -- sum )
  init-arr
  ITERS 0 do reverse-arr loop
  sum-arr ;

: main
  bench . cr ;
