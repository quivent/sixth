\ expect: 1
\ Pattern B04: dup 0> until
\ count up from -3 until positive — 4 iterations, exits at 1
: main -3 begin 1+ dup 0> until . cr ;
