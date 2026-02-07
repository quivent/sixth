\ expect: 2 1 4 3
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
  . space       \ 2
  . space       \ 1
  . space       \ 4
  .             \ 3
;
