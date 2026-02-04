\ expected: 19990000000
\ Merge two sorted arrays benchmark - 10K iterations

1000 constant SIZE
10000 constant ITERS
create arr1 SIZE cells allot
create arr2 SIZE cells allot
create merged SIZE 2 * cells allot

: arr1@ ( i -- n ) cells arr1 + @ ;
: arr1! ( n i -- ) cells arr1 + ! ;
: arr2@ ( i -- n ) cells arr2 + @ ;
: arr2! ( n i -- ) cells arr2 + ! ;
: merged@ ( i -- n ) cells merged + @ ;
: merged! ( n i -- ) cells merged + ! ;

: merge-arrays ( -- )
  0 0 0                   \ i j k
  begin
    over SIZE < 2 pick SIZE < or
  while
    2 pick SIZE >= if
      \ arr1 exhausted, take from arr2
      over arr2@ over merged!
      swap 1+ swap
    else over SIZE >= if
      \ arr2 exhausted, take from arr1
      2 pick arr1@ over merged!
      rot 1+ -rot
    else
      2 pick arr1@ over arr2@ >= if
        2 pick arr1@ over merged!
        rot 1+ -rot
      else
        over arr2@ over merged!
        swap 1+ swap
      then
    then then
    1+
  repeat
  2drop drop ;

: init-arrays ( -- )
  SIZE 0 do
    SIZE 2 * i - 1- i arr1!    \ descending
    SIZE i - 1- i arr2!        \ descending
  loop ;

: sum-merged ( -- n )
  0 SIZE 2 * 0 do i merged@ + loop ;

: bench ( -- sum )
  0
  ITERS 0 do
    init-arrays
    merge-arrays
    sum-merged +
  loop ;

: main
  bench . cr ;
