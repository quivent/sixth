\ Test 753: helper returning quotient and remainder
: divmod 2dup / rot rot mod ;
: main 17 5 divmod . . cr ;
