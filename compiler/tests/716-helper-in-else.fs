\ expect: 10
\ Test 716: helper called from inside else
: half 2/ ;
: main 0 if 99 . else 20 half . then cr ;
