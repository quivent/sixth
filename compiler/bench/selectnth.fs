\ expected: 4995000
\ Select nth element (quickselect) benchmark - 10K times

1000 constant SIZE
10000 constant ITERS
create arr SIZE cells allot
create work SIZE cells allot

: arr@ ( i -- n ) cells arr + @ ;
: arr! ( n i -- ) cells arr + ! ;
: work@ ( i -- n ) cells work + @ ;
: work! ( n i -- ) cells work + ! ;

: swap-work ( i j -- )
  over work@ over work@
  rot work! swap work! ;

: partition ( lo hi -- pivot-idx )
  2dup + 2 / work@        \ lo hi pivot
  rot rot                  \ pv lo hi
  begin
    2dup <=
  while
    begin 2 pick 3 pick work@ > while 1 under+ repeat
    begin 2 pick 2 pick work@ < while 1- repeat
    2dup <= if
      2dup swap-work
      1 under+ 1-
    then
  repeat
  nip nip ;

: quickselect ( k lo hi -- val )
  begin
    2dup < if
      2dup partition       \ k lo hi p
      dup 4 pick = if
        work@ nip nip nip exit
      then
      dup 4 pick < if
        \ k is in right partition
        rot drop 1+ swap   \ k p+1 hi
      else
        \ k is in left partition
        nip 1-             \ k lo p-1
      then
    else
      drop drop work@
      exit
    then
  again ;

: copy-to-work ( -- )
  SIZE 0 do i arr@ i work! loop ;

: select-kth ( k -- val )
  copy-to-work
  0 SIZE 1- quickselect ;

: init-arr ( -- )
  SIZE 0 do
    i 7 * 13 + SIZE mod i arr!
  loop ;

: bench ( -- sum )
  0
  ITERS 0 do
    i SIZE mod select-kth +
  loop ;

: main
  init-arr
  bench . cr ;
