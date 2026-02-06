\ Ackermann(3,10) benchmark - deep recursion
: ackermann ( m n -- result )
  over 0= if nip 1+ exit then
  dup 0= if drop 1- 1 recurse exit then
  over swap 1- recurse   \ m ack(m, n-1)
  swap 1- swap recurse ;

: main
  3 10 ackermann . cr ;
