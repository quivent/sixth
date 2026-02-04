\ expected: 499500000
\ Interpolation search benchmark - search 1M times in sorted array

1000 constant SIZE
1000000 constant ITERS
create arr SIZE cells allot

: arr@ ( i -- n ) cells arr + @ ;
: arr! ( n i -- ) cells arr + ! ;

: interp-search ( val -- idx|-1 )
  0 SIZE 1-             \ val lo hi
  begin
    2dup <= 3 pick 2 pick arr@ >= and 3 pick over arr@ <= and
  while
    \ pos = lo + ((val - arr[lo]) * (hi - lo)) / (arr[hi] - arr[lo])
    over arr@ 3 pick arr@ - dup 0= if
      \ arr[lo] == arr[hi]
      drop 2dup = if nip nip exit then
      2drop drop -1 exit
    then
    >r                    \ val lo hi ; R: arr[hi]-arr[lo]
    3 pick 2 pick arr@ - \ val lo hi (val-arr[lo])
    over 2 pick - *       \ val lo hi (val-arr[lo])*(hi-lo)
    r> /                  \ val lo hi pos-offset
    2 pick +              \ val lo hi pos
    dup arr@ 4 pick = if
      nip nip nip exit
    then
    dup arr@ 4 pick < if
      \ arr[pos] < val
      nip 1-              \ val lo pos-1
    else
      \ arr[pos] > val
      rot drop 1+ swap    \ val pos+1 hi
    then
  repeat
  2drop drop -1 ;

: init-arr ( -- )
  SIZE 0 do i i arr! loop ;

: bench ( -- sum )
  0
  ITERS 0 do
    i SIZE mod interp-search +
  loop ;

: main
  init-arr
  bench . cr ;
