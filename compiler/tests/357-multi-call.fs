\ expect: 20
\ Test 357: chained calls double->quadruple
: double dup + ;
: quadruple double double ;
: main 5 quadruple . cr ;
