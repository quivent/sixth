\ expected: 499500000
\ Binary search benchmark - search 1M times in sorted array

1000 constant SIZE
1000000 constant ITERS
create arr SIZE cells allot

: arr@ ( i -- n ) cells arr + @ ;
: arr! ( n i -- ) cells arr + ! ;

: bsearch ( val -- idx|-1 )
  0 SIZE 1-             \ val lo hi
  begin 2dup <= while
    2dup + 2 /          \ val lo hi mid
    dup arr@            \ val lo hi mid arr[mid]
    4 pick = if         \ found
      nip nip nip exit
    then
    dup arr@ 4 pick < if
      \ arr[mid] < val, search right half
      rot drop 1+ swap  \ val mid+1 hi ; lo=mid+1
    else
      \ arr[mid] > val, search left half
      nip 1- swap       \ val lo mid-1 ; hi=mid-1
    then
  repeat
  2drop drop -1 ;

: init-arr ( -- )
  SIZE 0 do i i arr! loop ;

: bench ( -- sum )
  0
  ITERS 0 do
    i SIZE mod bsearch +
  loop ;

: main
  init-arr
  bench . cr ;
