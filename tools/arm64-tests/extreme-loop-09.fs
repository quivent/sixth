\ expect: 55
\ Negative limit with negative start
: main
  0
  0 -10 do          \ limit=0, start=-10
    i negate +      \ add absolute value of i
  loop
;
\ i = -10, -9, -8, -7, -6, -5, -4, -3, -2, -1 (10 iterations)
\ negated: 10, 9, 8, 7, 6, 5, 4, 3, 2, 1
\ sum = 55
