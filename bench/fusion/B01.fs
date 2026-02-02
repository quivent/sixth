\ expect: 0
\ Pattern B01: dup 0> while
\ countdown from 5 while positive — 5 iterations, exits at 0
: main 5 begin dup 0> while 1- repeat . cr ;
