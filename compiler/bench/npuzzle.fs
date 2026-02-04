\ expected: 11111
\ N-puzzle (8-puzzle) - count inversions to check solvability

9 constant SIZE
create puzzle SIZE cells allot

: p@ ( i -- val ) cells puzzle + @ ;
: p! ( val i -- ) cells puzzle + ! ;

: count-inversions ( -- count )
  0
  SIZE 0 do
    SIZE i 1+ do
      j p@ dup 0> if
        i p@ dup 0> if
          over > if swap 1+ swap then
        else drop then
      else drop then
    loop
  loop ;

: is-solvable? ( -- flag )
  count-inversions 1 and 0= ;

: init-puzzle ( seed -- )
  \ Simple scramble based on seed
  SIZE 0 do i 1+ i p! loop  \ 1 2 3 4 5 6 7 8 0
  0 SIZE 1- p!              \ put 0 at end

  \ Swap based on seed
  dup SIZE mod p@
  over SIZE mod
  over 1+ SIZE mod p@ swap
  2 pick SIZE mod p!
  swap 1+ SIZE mod p! drop ;

: main
  0
  100000 0 do
    i init-puzzle
    is-solvable? if 1+ then
  loop
  . cr ;
