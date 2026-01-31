\ expect: 33
\ Test 529: recursive helper called from loop
: fact dup 1 > if dup 1- fact * then ;
: main 0 5 1 do i fact + loop . cr ;
