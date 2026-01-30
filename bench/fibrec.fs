\ fibrec.fs - Recursive countdown (tests call overhead)
\ NOTE: Double-recurse fib broken - stack not preserved across calls
: countdown ( n -- 0 )
  dup 0= if exit then
  1- recurse ;
: main ( -- ) 100000 countdown . cr ;
