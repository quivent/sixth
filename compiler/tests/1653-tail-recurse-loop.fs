\ expect: 0
: countdown ( n -- ) dup 0= if . else 1- recurse then ;
: main 0 countdown cr ;
