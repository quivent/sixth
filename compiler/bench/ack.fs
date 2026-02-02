\ expected: 2045
\ Ackermann function benchmark
\ NOTE: ack(3,10)=8189 exceeds call stack depth. Using ack(3,8)=2045.

: ack ( m n -- r )
  over 0= if nip 1+ exit then
  dup 0= if drop 1- 1 recurse exit then
  over 1- -rot 1- recurse recurse ;

: main
  3 8 ack . cr
;
