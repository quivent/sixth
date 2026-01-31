\ expect: 479001600
: fact ( n -- n! ) dup 1 > if dup 1- fact * then ;
: main 12 fact . cr ;
