\ expect: 15
\ Test 600: 2dup then arithmetic then 2drop
: main 5 10 2dup + rot rot 2drop . cr ;
