\ expect: 106
\ Test 627: accumulator below nested loops preserved
: main 100 0 3 begin dup 0> while 2 begin dup 0> while 1- rot 1+ rot rot repeat drop 1- repeat drop + . cr ;
