\ expected: 100000000
\ Tail call optimization - iterative version

: countdown ( n -- acc )
  0 swap   \ acc n
  begin dup 0> while
    swap 1+ swap 1-
  repeat drop ;

: main
  100000000 countdown . cr ;
