\ Test 348: count digits in 12345
: digits 0 swap begin swap 1+ swap 10 / dup 0= until drop ;
: main 12345 digits . cr ;
