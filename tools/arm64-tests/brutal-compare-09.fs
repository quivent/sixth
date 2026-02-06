\ expect: 0
\ Test: Chained comparisons and complex boolean expressions

: main
  \ Chained: (a < b) AND (b < c)
  1 2 < 2 3 < and -1 <> if 1 exit then      \ 1<2 AND 2<3 = true
  2 1 < 2 3 < and 0 <> if 2 exit then       \ 2<1 AND 2<3 = false

  \ Range check: (min <= x) AND (x <= max)
  5 0 >= 5 10 <= and -1 <> if 3 exit then   \ 0 <= 5 <= 10
  -1 0 >= -1 10 <= and 0 <> if 4 exit then  \ -1 out of [0,10]
  11 0 >= 11 10 <= and 0 <> if 5 exit then  \ 11 out of [0,10]

  \ Complex: (a = b) OR (a = c)
  5 5 = 5 10 = or -1 <> if 6 exit then      \ 5=5 OR 5=10
  5 3 = 5 10 = or 0 <> if 7 exit then       \ 5=3 OR 5=10 = false

  \ Nested: ((a < b) AND (c < d)) OR (e = f)
  1 2 < 3 4 < and  5 6 = or -1 <> if 8 exit then    \ (1<2 AND 3<4) OR 5=6
  2 1 < 4 3 < and  5 5 = or -1 <> if 9 exit then    \ (false AND false) OR true

  0
;
