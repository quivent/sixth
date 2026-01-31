\ expect: 12
\ Test 715: helper called from inside if
: triple 3 * ;
: main 1 if 4 triple . then cr ;
