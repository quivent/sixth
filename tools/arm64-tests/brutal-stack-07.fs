\ expect: 0
\ Test: Deep stack stress - 8 items then complex shuffle
\ Tests if stack pointer management is correct at depth

: main
  1 2 3 4 5 6 7 8     ( 8 items deep )
  drop drop drop drop  ( remove 8 7 6 5 )
  ( Stack: 1 2 3 4 )
  rot                 ( Stack: 1 3 4 2 )
  drop drop           ( Stack: 1 3 )
  swap                ( Stack: 3 1 )
  1 - swap 3 - or
;
