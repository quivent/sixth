\ arith.fs - Two-variable arithmetic (10M iterations)
: main ( -- ) 0 10000000 begin swap 1+ swap 1-nzloop drop . cr ;
