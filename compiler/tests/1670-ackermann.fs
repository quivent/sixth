\ expect: 9
\ Ackermann function A(2,3) = 9
: ack ( m n -- result )
  over 0= if nip 1+ exit then
  dup 0= if drop 1- 1 ack exit then
  \ m>0, n>0: A(m-1, A(m, n-1))
  over swap          \ ( m m n )
  1- ack             \ ( m A(m,n-1) )
  swap 1- swap ack ; \ ( A(m-1, A(m,n-1)) )
: main 2 3 ack . cr ;
