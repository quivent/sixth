\ expect: 42
\ Test: if-else after loop completes → 42
: main 3 begin 1- dup 0= until drop 42 0 > if 42 else 99 then . cr ;
