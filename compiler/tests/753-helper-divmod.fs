\ expect: 2 3
\ Test 753: helper returning quotient and remainder
: divmod ( a b -- q r ) 2dup / rot rot mod ;
: main 17 5 divmod . . cr ;
