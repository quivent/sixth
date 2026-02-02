\ expect: 4
\ Pattern C04: 1- dup 0> until
\ 5→4 (4>0=true, exit immediately) — until exits on TRUE
: main 5 begin 1- dup 0> until . cr ;
