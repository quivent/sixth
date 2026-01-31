\ expect: 99
: go ( n -- n ) dup 0= if drop 99 exit then 1- recurse ;
: main 0 go . cr ;
