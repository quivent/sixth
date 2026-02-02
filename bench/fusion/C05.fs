\ expect: 0
\ Pattern C05: 1- dup 0= until
\ 5→4→3→2→1→0 (0=true, exit) — 5 iterations
: main 5 begin 1- dup 0= until . cr ;
