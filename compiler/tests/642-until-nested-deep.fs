\ expect: 1
\ Test 642: nested begin/until - inner counts to 3 each time
: main 0 1 begin swap 1 begin 1+ dup 3 = until drop swap 1+ dup 5 > until drop 1+ . cr ;
