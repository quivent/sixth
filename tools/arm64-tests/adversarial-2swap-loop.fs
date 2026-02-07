\ expect: 4 3 2 1
\ Test 2swap in a loop - 4 iterations returns to original order
\ This catches register state corruption across loop iterations
: main
  1 2 3 4
  4 0 do
    2swap
  loop
  \ After 4 swaps: back to 1 2 3 4
  . space       \ 4
  . space       \ 3
  . space       \ 2
  .             \ 1
;
