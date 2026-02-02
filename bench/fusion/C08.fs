\ expect: 1
\ Pattern C08: 1+ dup 0= while
\ -1→0 (0=true, continue)→1 (0=false, exit)
: main -1 begin 1+ dup 0= while repeat . cr ;
