\ expect: -1
\ Test: invert inside if body → -1
: main 0 dup 0= if invert then . cr ;
