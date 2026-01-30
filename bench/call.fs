\ call.fs - Function call overhead (10M calls)
: dec1 ( n -- n ) 1- ;
: main ( -- ) 10000000 begin dec1 dup 0= until . cr ;
