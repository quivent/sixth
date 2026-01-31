\ expect: 42
\ Test 328: absolute value
: myabs dup 0< if negate then ;
: main -42 myabs . cr ;
