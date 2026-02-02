\ expect: -1
\ Pattern C12: 1+ dup 0< until
\ -2→-1 (0<true, exit) — 1 iteration
: main -2 begin 1+ dup 0< until . cr ;
