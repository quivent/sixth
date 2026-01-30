\ call.fs - Recursive call overhead (100K calls with TCO)
: countdown ( n -- 0 )
  dup 0= if exit then
  1- recurse ;
: main ( -- ) 100000 countdown . cr ;
