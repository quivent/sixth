\ Test 505: recursive count up to N
: countup 2dup > if swap dup . 1+ swap countup else 2drop then ;
: main 1 5 countup cr ;
