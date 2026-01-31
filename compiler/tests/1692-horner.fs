\ expect: 123
\ Horner's method: evaluate 2x^3 + 3x^2 + x + 3 at x=4
\ ((2*4 + 3)*4 + 1)*4 + 3 = (11)*4+1 = 45*4+3 = 44+1... let me redo
\ ((2)*4 + 3) = 11. (11*4 + 1) = 45. (45*4 + 3) = 183.
\ Hmm let me pick simpler: x^2 + 2x + 3 at x=10 = 100+20+3 = 123
create co 24 allot
: main
  1 co 0 cells + !    \ x^2 coefficient
  2 co 1 cells + !    \ x coefficient
  3 co 2 cells + !    \ constant
  co @ 10 *           \ 1*10 = 10
  co 1 cells + @ +    \ 10+2 = 12
  10 *                \ 120
  co 2 cells + @ +    \ 123
  . cr ;
