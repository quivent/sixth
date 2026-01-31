\ expect: 3 7
\ Push 7 and 3, use rstack + comparison to print min then max
: main
  7 >r 3 >r
  r> r> 2dup min . max . cr ;
