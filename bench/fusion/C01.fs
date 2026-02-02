\ expect: 0
\ Pattern C01: 1- dup 0> while
\ 5→4→3→2→1→0 (0>false, exit) — the standard countdown
: main 5 begin 1- dup 0> while repeat . cr ;
