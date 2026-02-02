\ expect: 0
\ Pattern C09: 1+ dup 0< while
\ -3→-2→-1→0 (0<false, exit) — 3 iterations
: main -3 begin 1+ dup 0< while repeat . cr ;
