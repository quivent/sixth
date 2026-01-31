\ expect: 11
\ Test 725: 3-deep chain main->A->B->C
: add5 5 + ;
: dbl-add5 2* add5 ;
: main 3 dbl-add5 . cr ;
