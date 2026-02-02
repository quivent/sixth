\ expect: 0
\ Pattern C11: 1+ dup 0= until
\ -3→-2→-1→0 (0=true, exit) — 3 iterations
: main -3 begin 1+ dup 0= until . cr ;
