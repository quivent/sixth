\ expect: 13
\ Test 742: chain of pure words whose final result is used
: a 2* ;
: b 3 + ;
: c a b ;
: main 5 c . cr ;
