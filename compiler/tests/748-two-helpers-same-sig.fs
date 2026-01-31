\ expect: 11 9
\ Test 748: two helpers with same signature
: add1 1+ ;
: sub1 1- ;
: main 10 add1 . 10 sub1 . cr ;
