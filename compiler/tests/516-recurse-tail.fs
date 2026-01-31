\ Test 516: tail position recurse (last thing before then)
: tailr dup 0 > if 1- tailr then ;
: main 10 tailr . cr ;
