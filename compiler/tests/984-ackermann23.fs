\ expect: 9
\ Test 984: Ackermann(2,3) = 9
\ A(0,n)=n+1  A(m,0)=A(m-1,1)  A(m,n)=A(m-1,A(m,n-1))
\ Stack: ( m n -- result )
: ack ( m n -- r ) over 0= if nip 1+ exit then dup 0= if drop 1- 1 ack exit then over 1- rot rot 1- ack ack ;
: main 2 3 ack . cr ;
