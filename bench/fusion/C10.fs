\ expect: 1
\ Pattern C10: 1+ dup 0> until
\ -3→-2→-1→0→1 (0>true, exit) — 4 iterations
: main -3 begin 1+ dup 0> until . cr ;
