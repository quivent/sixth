\ expect: 1
\ Pattern C03: 1- dup 0< while
\ 0→-1 (0<true, +3→2)→1 (0<false, exit)
: main 0 begin 1- dup 0< while 3 + repeat . cr ;
