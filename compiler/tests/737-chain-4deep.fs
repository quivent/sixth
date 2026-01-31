\ expect: 4
\ Test 737: 4-deep call chain
: a 1+ ;
: b a a ;
: c b b ;
: main 0 c . cr ;
