\ expect: 86
\ Evaluate 3x^2 + 2x + 1 at x=5 using Horner's method
\ Horner: ((3)*5 + 2)*5 + 1 = (15+2)*5+1 = 17*5+1 = 85+1 = 86
create coeff 24 allot
: co@ ( i -- val ) 8 * coeff + @ ;
: co! ( val i -- ) 8 * coeff + ! ;
variable hx
: horner ( x n -- result )
  swap hx !
  \ coefficients stored high to low: coeff[0]=leading
  0 co@              \ start with leading coefficient
  swap 1 do          \ loop from 1 to n-1
    hx @ * i co@ +   \ result = result * x + coeff[i]
  loop ;
: main
  3 0 co!  2 1 co!  1 2 co!
  5 3 horner . cr ;
