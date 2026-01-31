\ expect: 1
: iseven ( n -- f ) dup 0= if drop 1 else 1- dup 0= if drop 0 else 1- recurse then then ;
: main 10 iseven . cr ;
