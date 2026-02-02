\ expect: 0
\ Pattern C07: 1+ dup 0> while
\ -1→0 (0>false, exit immediately)
: main -1 begin 1+ dup 0> while repeat . cr ;
