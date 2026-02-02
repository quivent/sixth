\ expect: -1
\ Pattern B05: dup 0< until
\ count down from 3 until negative — 4 iterations, exits at -1
: main 3 begin 1- dup 0< until . cr ;
