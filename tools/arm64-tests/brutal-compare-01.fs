\ expect: 0
\ Test: Basic equality and inequality with zero
\ Forth semantics: = returns -1 for true, 0 for false

: main
  0 0 = -1 <> if 1 exit then      \ 0 = 0 must be true (-1)
  1 1 = -1 <> if 2 exit then      \ 1 = 1 must be true (-1)
  1 0 = 0 <> if 3 exit then       \ 1 = 0 must be false (0)
  0 1 = 0 <> if 4 exit then       \ 0 = 1 must be false (0)

  0 0 <> 0 <> if 5 exit then      \ 0 <> 0 must be false (0)
  1 0 <> -1 <> if 6 exit then     \ 1 <> 0 must be true (-1)
  -1 -1 = -1 <> if 7 exit then    \ -1 = -1 must be true (-1)
  -1 0 <> -1 <> if 8 exit then    \ -1 <> 0 must be true (-1)

  0   \ exit code 0 = all tests passed
;
