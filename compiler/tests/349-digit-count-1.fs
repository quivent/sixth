\ expect: 1
\ Test 349: count digits in 1
: digits 0 swap begin swap 1+ swap 10 / dup 0= until drop ;
: main 1 digits . cr ;
