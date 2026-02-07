\ expect: 200
\ EDGE: Rapid alternating @ and ! to same location
\ Tests: Write and read same cell 100 times with changing values
\ This stresses the memory instruction encoding and ensures no instruction
\ reordering issues between loads and stores

create cell1 8 allot

: main
  0 cell1 !        \ Initialize to 0

  \ Do 100 iterations of: read, increment, write back
  \ This tests rapid alternating @ and ! to exact same address
  100 0 do
    cell1 @        \ read current value
    1 +            \ increment
    cell1 !        \ write back
  loop

  \ Final value should be 100
  cell1 @

  \ Add another round of 100 to stress test more
  100 0 do
    cell1 @
    1 +
    cell1 !
  loop

  cell1 @          \ Should be 200
;
