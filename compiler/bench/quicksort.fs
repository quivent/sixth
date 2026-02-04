\ expected: 49995000
\ Quicksort benchmark - sort 10000 elements, sum result

10000 constant SIZE
create arr SIZE cells allot

: arr@ ( i -- n ) cells arr + @ ;
: arr! ( n i -- ) cells arr + ! ;

: swap-arr ( i j -- )
  over arr@ over arr@   \ i j a[i] a[j]
  rot arr!              \ i a[j] ; a[j]->a[i]
  swap arr! ;           \ a[i]->a[j]

: partition ( lo hi -- pivot-idx )
  2dup + 2 / arr@       \ lo hi pivot-value
  rot rot               \ pv lo hi
  begin
    2dup <=
  while
    begin 2 pick 3 pick arr@ > while 1 under+ repeat   \ increment lo
    begin 2 pick 2 pick arr@ < while 1- repeat         \ decrement hi
    2dup <= if
      2dup swap-arr
      1 under+ 1-
    then
  repeat
  nip nip ;

: qsort ( lo hi -- )
  2dup < if
    2dup partition      \ lo hi p
    rot over 1- recurse \ hi p ; qsort(lo, p-1)
    swap 1+ swap recurse \ qsort(p+1, hi)
  else
    2drop
  then ;

: init-arr ( -- )
  SIZE 0 do
    SIZE i - 1- i arr!
  loop ;

: sum-arr ( -- n )
  0 SIZE 0 do i arr@ + loop ;

: main
  init-arr
  0 SIZE 1- qsort
  sum-arr . cr ;
