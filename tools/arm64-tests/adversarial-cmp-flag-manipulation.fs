\ Adversarial: Boolean flag manipulation
\ Tests logical operations on comparison results
\ expect: 1

: bool-not ( flag -- flag' )
  \ Logical NOT: -1 -> 0, 0 -> -1
  0= ;

: bool-and ( f1 f2 -- f )
  and ;

: bool-or ( f1 f2 -- f )
  or ;

: main
  \ NOT of true = false
  -1 bool-not
  0= 0= if 0 exit then

  \ NOT of false = true
  0 bool-not
  -1 = 0= if 0 exit then

  \ AND truth table
  -1 -1 bool-and -1 = 0= if 0 exit then
  -1 0 bool-and 0= 0= if 0 exit then
  0 -1 bool-and 0= 0= if 0 exit then
  0 0 bool-and 0= 0= if 0 exit then

  \ OR truth table
  -1 -1 bool-or -1 = 0= if 0 exit then
  -1 0 bool-or -1 = 0= if 0 exit then
  0 -1 bool-or -1 = 0= if 0 exit then
  0 0 bool-or 0= 0= if 0 exit then

  \ XOR for inequality: a XOR b = (a AND NOT b) OR (NOT a AND b)
  \ But we can just use xor directly
  -1 -1 xor 0= 0= if 0 exit then   \ same -> 0
  -1 0 xor -1 = 0= if 0 exit then  \ different -> -1
  0 -1 xor -1 = 0= if 0 exit then  \ different -> -1
  0 0 xor 0= 0= if 0 exit then     \ same -> 0

  \ Double negation
  -1 bool-not bool-not
  -1 = 0= if 0 exit then

  0 bool-not bool-not
  0= 0= if 0 exit then

  \ Combining comparisons
  5 5 = 3 3 = and    \ true AND true = true
  -1 = 0= if 0 exit then

  5 5 = 3 4 = and    \ true AND false = false
  0= 0= if 0 exit then

  1 ;
