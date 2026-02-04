\ expected: 499500000
\ Jump search benchmark - search 1M times in sorted array

1000 constant SIZE
1000000 constant ITERS
31 constant STEP
create arr SIZE cells allot

: arr@ ( i -- n ) cells arr + @ ;
: arr! ( n i -- ) cells arr + ! ;

: jump-search ( val -- idx|-1 )
  0                       \ val prev
  begin
    dup STEP + SIZE min   \ val prev curr
    dup 1- arr@ 2 pick <
    over SIZE < and
  while
    nip                   \ val curr ; prev=curr
  repeat
  \ linear search from prev to min(curr, SIZE)
  SIZE min swap           \ val hi prev
  begin 2dup > while
    dup arr@ 2 pick = if
      nip nip exit
    then
    1+
  repeat
  2drop drop -1 ;

: init-arr ( -- )
  SIZE 0 do i i arr! loop ;

: bench ( -- sum )
  0
  ITERS 0 do
    i SIZE mod jump-search +
  loop ;

: main
  init-arr
  bench . cr ;
