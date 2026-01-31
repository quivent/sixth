\ expect: 120
\ Test: factorial 5! in loop → 120
: main 1 5 begin dup 1 > while swap over * swap 1- repeat drop . cr ;
