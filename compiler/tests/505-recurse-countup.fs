\ expect: 1 2 3 4 5
\ Test 505: recursive count up to N
: countup ( start end -- ) 2dup > if 2drop else swap dup . 1+ swap countup then ;
: main ( -- ) 1 5 countup cr ;
