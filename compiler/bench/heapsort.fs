\ expected: 49995000
\ Heapsort benchmark - sort 10000 elements, sum result

10000 constant SIZE
create arr SIZE cells allot

: arr@ ( i -- n ) cells arr + @ ;
: arr! ( n i -- ) cells arr + ! ;

: swap-arr ( i j -- )
  over arr@ over arr@
  rot arr! swap arr! ;

: sift-down ( start end -- )
  begin
    dup 2* 1+           \ start end child
    2dup >= if drop 2drop exit then
    dup 1+ 2 pick <= if
      dup arr@ over 1+ arr@ < if 1+ then
    then
    dup arr@ 2 pick arr@ >= if drop 2drop exit then
    2 pick over swap-arr
    rot drop swap
  again ;

: heapify ( -- )
  SIZE 2 / 1-
  begin dup 0>= while
    dup SIZE 1- sift-down
    1-
  repeat drop ;

: hsort ( -- )
  heapify
  SIZE 1-
  begin dup 0> while
    0 over swap-arr
    0 over 1- sift-down
    1-
  repeat drop ;

: init-arr ( -- )
  SIZE 0 do
    SIZE i - 1- i arr!
  loop ;

: sum-arr ( -- n )
  0 SIZE 0 do i arr@ + loop ;

: main
  init-arr
  hsort
  sum-arr . cr ;
