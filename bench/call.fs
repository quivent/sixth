\ call.fs - Recursive call overhead (10M calls with TCO)
: countdown ( n -- 0 )
  dup 0= if exit then
  1- recurse ;
: main ( -- ) 10000000 countdown . cr ;
