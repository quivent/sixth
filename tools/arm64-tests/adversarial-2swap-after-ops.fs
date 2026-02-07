\ expect: 0
\ Test 2swap after complex stack operations that stress register allocation
\ Start: push 1 2 3 4 5, manipulate heavily, then 2swap
: main
  1 2 3 4 5
  drop          \ 1 2 3 4
  swap          \ 1 2 4 3
  rot           \ 1 4 3 2
  -rot          \ 1 2 4 3 (back)
  over          \ 1 2 4 3 4
  drop          \ 1 2 4 3
  swap          \ 1 2 3 4
  2swap         \ 3 4 1 2
  \ Stack: 3 4 1 2 (TOS=2)
  2 - abs             \ TOS should be 2
  swap 1 - abs +      \ next should be 1
  swap 4 - abs +      \ next should be 4
  swap 3 - abs +      \ next should be 3
;
