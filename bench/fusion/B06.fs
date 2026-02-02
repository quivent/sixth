\ expect: 0
\ Pattern B06: dup 0= until
\ count down from 5 until zero — 5 iterations, exits at 0
: main 5 begin 1- dup 0= until . cr ;
