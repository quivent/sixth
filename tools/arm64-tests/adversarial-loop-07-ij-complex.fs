\ adversarial-loop-07-ij-complex.fs - I and J in complex nesting
\ Sum of i+j for j=0,1,2 and i=0,1,2,3
\ j=0: 0+1+2+3=6, j=1: 1+2+3+4=10, j=2: 2+3+4+5=14
\ Total = 6+10+14 = 30
\ expect: 30

: main
  0  \ accumulator
  3 0 do          \ j = 0,1,2
    4 0 do        \ i = 0,1,2,3
      i j + +     \ accumulate i+j
    loop
  loop
;
