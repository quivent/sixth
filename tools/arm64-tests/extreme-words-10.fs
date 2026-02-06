\ expect: 99
\ Extreme Test 10: 3-way mutual recursion with convergence
\ Tests: complex call graph, multiple entry points

: ping ( n -- result )
  dup 0= if drop 33 exit then
  1 - pong ;

: pong ( n -- result )
  dup 0= if drop 33 exit then
  1 - pang ;

: pang ( n -- result )
  dup 0= if drop 33 exit then
  1 - ping ;

: main
  3 ping 6 pong + 9 pang + ;
