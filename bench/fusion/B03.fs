\ expect: 1
\ Pattern B03: dup 0= while
\ only one iteration — 0 is 0=true, 1+ makes 1, 1 is 0=false
: main 0 begin dup 0= while 1+ repeat . cr ;
