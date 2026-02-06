\ expect: 45
\ STRESS: CREATE buffer in nested loops with computed addresses
\ Tests: Nested loop accessing buffer using j for outer index

create arr 80 allot   \ 10 cells

: main
  \ Fill with 0-9
  10 0 do
    i arr i 8 * + !
  loop
  \ Sum with nested loop - use j for outer loop index
  0
  10 0 do
    1 0 do          \ inner loop runs once
      arr j 8 * + @ +   \ j is outer loop index
    loop
  loop
  \ 0+1+2+3+4+5+6+7+8+9 = 45
;
