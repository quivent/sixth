\ expected: 8189
\ Ackermann(3,10)

: ackmed ( m n -- r ) recursive
  over 0= if nip 1+ exit then
  dup 0= if drop 1- 1 recurse exit then
  over 1- -rot 1- recurse recurse ;

: main
  3 10 ackmed . cr ;
