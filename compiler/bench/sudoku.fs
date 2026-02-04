\ expected: 100000
\ Sudoku validator - check if a board is valid

create board 81 allot

: b@ ( row col -- val ) swap 9 * + board + c@ ;
: b! ( val row col -- ) swap 9 * + board + c! ;

: clear-board ( -- )
  81 0 do 0 board i + c! loop ;

: check-row ( row -- valid? )
  0 9 0 do
    over i b@ dup 0> if
      1 swap lshift over and 0<> if 2drop 0 unloop exit then
      or
    else drop then
  loop nip 1 ;

: check-col ( col -- valid? )
  0 9 0 do
    i 2 pick b@ dup 0> if
      1 swap lshift over and 0<> if 2drop 0 unloop exit then
      or
    else drop then
  loop nip 1 ;

: valid-board? ( -- flag )
  9 0 do i check-row 0= if 0 unloop exit then loop
  9 0 do i check-col 0= if 0 unloop exit then loop
  1 ;

: init-board ( -- )
  clear-board
  5 0 0 b!  3 0 1 b!  7 0 4 b!
  6 1 0 b!  1 1 3 b!  9 1 4 b!  5 1 5 b!
  9 2 1 b!  8 2 2 b!  6 2 7 b!
  8 3 0 b!  6 3 4 b!  3 3 8 b!
  4 4 0 b!  8 4 3 b!  3 4 5 b!  1 4 8 b!
  7 5 0 b!  2 5 4 b!  6 5 8 b!
  6 6 1 b!  2 6 6 b!  8 6 7 b!
  4 7 3 b!  1 7 4 b!  9 7 5 b!  5 7 8 b!
  8 8 4 b!  7 8 7 b!  9 8 8 b! ;

: main
  0
  100000 0 do
    init-board
    valid-board? if 1+ then
  loop
  . cr ;
