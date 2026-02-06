\ Adversarial: Test 0>= and 0<= after dup/over operations
\ expect: 1
: main
  \ dup then check both copies
  5 dup            \ 5 5
  0>= swap         \ flag 5
  0>= and          \ both should be true
  -1 = if
    \ over test
    -3 7 over      \ -3 7 -3
    0<=            \ -3 7 flag
    swap drop swap \ flag -3
    0<=            \ flag flag
    and            \ both true
    -1 = if 1 else 0 then
  else 0 then ;
