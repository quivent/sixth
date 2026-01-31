\ expect: 35
\ Multiply by 7 using shifts: x*7 = x*8 - x = (x<<3) - x
: times7 ( n -- n*7 ) dup 3 lshift swap - ;
: main 5 times7 . cr ;
