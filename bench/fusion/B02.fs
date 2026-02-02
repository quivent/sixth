\ expect: 0
\ Pattern B02: dup 0< while
\ count up from -3 while negative — 3 iterations, exits at 0
: main -3 begin dup 0< while 1+ repeat . cr ;
