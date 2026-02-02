\ Ackermann function benchmark
\ Tests: ack(3,10)=8189, ack(4,1)=65533

: ack ( m n -- r ) recursive
  over 0= if nip 1+ exit then
  dup 0= if drop 1- 1 recurse exit then
  over 1- -rot 1- recurse recurse ;

: main
  3 10 ack . cr
  4 1 ack . cr
;
