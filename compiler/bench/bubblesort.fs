\ expected: 499500
\ Bubble sort benchmark - sort 1000 elements descending, sum result

1000 constant SIZE
create arr SIZE cells allot

: arr@ ( i -- n ) cells arr + @ ;
: arr! ( n i -- ) cells arr + ! ;

: swap-arr ( i j -- )
  over arr@ over arr@
  rot arr! swap arr! ;

: bubble-sort ( n -- )
  dup 0 do
    dup 1- i - 0 do
      i arr@ i 1+ arr@ > if
        i i 1+ swap-arr
      then
    loop
  loop drop ;

: init-arr ( -- )
  SIZE 0 do SIZE i - 1- i arr! loop ;

: sum-arr ( -- n )
  0 SIZE 0 do i arr@ + loop ;

: main ( -- )
  init-arr
  SIZE bubble-sort
  sum-arr . cr ;
