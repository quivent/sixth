\ Test 457: dup 0< if negate - flags corruption regression
\ Expected output: 5
: main -5 dup 0< if negate then . cr ;
