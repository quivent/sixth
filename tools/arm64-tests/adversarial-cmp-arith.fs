\ Adversarial: Test 0>= and 0<= with arithmetic results
\ expect: 1
: main
  \ 5 - 3 = 2 >= 0
  5 3 - 0>=
  -1 = if
    \ 3 - 5 = -2 <= 0
    3 5 - 0<=
    -1 = if
      \ -10 + 5 = -5 <= 0
      -10 5 + 0<=
      -1 = if
        \ 10 - 5 = 5 >= 0
        10 5 - 0>=
        -1 = if 1 else 0 then
      else 0 then
    else 0 then
  else 0 then ;
