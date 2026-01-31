\ expect: 6
\ nzloop with inner do/loop: multiply by doing repeated addition
\ Compute 2*3 by adding 2 three times
: mul2x3 0 3 begin swap 2 + swap 1- nzloop drop ;
: main mul2x3 . cr ;
