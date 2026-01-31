\ expect: 30 20 10
\ Test 683: . in if with deep stack
: main 10 20 30 1 if . then . . cr ;
