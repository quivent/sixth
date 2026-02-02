\ expect: 0
: countdown ( n -- 0 ) recursive dup 0= if exit then 1- recurse ;
: main 100 countdown . cr ;
