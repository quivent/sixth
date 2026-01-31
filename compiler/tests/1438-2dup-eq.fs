\ expect: -1 5 5
\ Test 1438: 2dup = — compare top two without consuming
\ 5 5 → 2dup → 5 5 5 5 → = → 5 5 -1 (true)
\ Print: -1 5 5
: main 5 5 2dup = . . . cr ;
