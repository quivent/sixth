\ expect: 24
\ Test 358: chained calls double->quadruple->octuple
: double dup + ;
: quadruple double double ;
: octuple double quadruple ;
: main 3 octuple . cr ;
