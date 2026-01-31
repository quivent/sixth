\ expect: 20
\ Test 988: nested word calls 5 deep, each modifying a value
: f5 2+ ;
: f4 f5 3 + ;
: f3 f4 4 + ;
: f2 f3 5 + ;
: f1 f2 6 + ;
: main 0 f1 . cr ;
