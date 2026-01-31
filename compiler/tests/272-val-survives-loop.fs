\ expect: 42
\ Test: value on stack survives loop → 42
: main 42 3 begin 1- dup 0= until drop . cr ;
