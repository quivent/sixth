\ expect: 0
\ Test 2swap in a loop - 4 iterations returns to original order
\ This catches register state corruption across loop iterations
: main
  1 2 3 4
  4 0 do
    2swap
  loop
  \ After 4 swaps: back to 1 2 3 4
  \ Stack: 1 2 3 4 (TOS=4)
  4 - abs             \ TOS should be 4
  swap 3 - abs +      \ next should be 3
  swap 2 - abs +      \ next should be 2
  swap 1 - abs +      \ next should be 1
;
