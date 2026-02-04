\ expected: 49995000
\ Mergesort benchmark - sort 10000 elements, sum result

10000 constant SIZE
create arr SIZE cells allot
create tmp SIZE cells allot

: arr@ ( i -- n ) cells arr + @ ;
: arr! ( n i -- ) cells arr + ! ;
: tmp@ ( i -- n ) cells tmp + @ ;
: tmp! ( n i -- ) cells tmp + ! ;

: merge ( lo mid hi -- )
  rot                   \ mid hi lo
  dup >r                \ save lo as k
  over 1+ rot           \ hi lo mid+1 ; i=lo j=mid+1
  begin                 \ hi i j
    2 pick r@ <=
  while
    over 3 pick <= over 2 pick arr@ >= and if
      \ i <= mid and arr[i] >= arr[j]
      over arr@ r@ tmp!
      swap 1+ swap
    else 2 pick 4 pick <= if
      \ j <= hi
      dup arr@ r@ tmp!
      1+
    else
      over arr@ r@ tmp!
      swap 1+ swap
    then then
    r> 1+ >r
  repeat
  r> drop 2drop drop
  \ copy tmp back
  SIZE 0 do tmp@ i arr! loop ;

: msort ( lo hi -- )
  2dup < if
    2dup + 2 /          \ lo hi mid
    2 pick over recurse \ msort(lo, mid)
    dup 1+ 2 pick recurse \ msort(mid+1, hi)
    -rot swap merge
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
  0 SIZE 1- msort
  sum-arr . cr ;
