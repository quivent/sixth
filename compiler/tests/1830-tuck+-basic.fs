\ expect: 8 3
\ tuck+ effect: ( a b -- b a+b ) via xadd
\ Stack: 5 3 -> tuck+ -> ( 3 8 )
\ . prints TOS=8, . prints NOS=3
: main 5 3 tuck+ . . cr ;
