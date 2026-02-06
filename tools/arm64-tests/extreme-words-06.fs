\ expect: 105
\ Extreme Test 06: Words with confusing similar names
\ Tests: symbol table hashing, name comparison edge cases

: test 10 ;
: test1 11 ;
: test2 20 ;
: test10 30 ;
: test11 40 ;
: tes 1 ;
: te 2 ;
: t 3 ;

: main
  test test1 + test2 + test10 + test11 +
  tes - te - t - ;
