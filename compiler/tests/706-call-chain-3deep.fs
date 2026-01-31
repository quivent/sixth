\ expect: 16
\ Test 706: 3-deep call chain B does math
: add3 3 + ;
: add3twice add3 add3 ;
: main 10 add3twice . cr ;
