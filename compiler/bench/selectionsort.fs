\ expected: 499500
\ Selection sort benchmark - sort 1000 elements, sum result

1000 constant SIZE
create arr SIZE cells allot

: arr@ ( i -- n ) cells arr + @ ;
: arr! ( n i -- ) cells arr + ! ;

: swap-arr ( i j -- )
  over arr@ over arr@
  rot arr! swap arr! ;

: find-min ( start end -- min-idx )
  over >r
  swap 1+ do
    i arr@ r@ arr@ < if r> drop i >r then
  loop r> ;

: selection-sort ( n -- )
  0 do
    i SIZE find-min
    i over <> if i swap-arr else drop then
  loop ;

: init-arr ( -- )
  SIZE 0 do SIZE i - 1- i arr! loop ;

: sum-arr ( -- n )
  0 SIZE 0 do i arr@ + loop ;

: main ( -- )
  init-arr
  SIZE selection-sort
  sum-arr . cr ;
