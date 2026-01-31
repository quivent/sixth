\ expect: 13 8
\ tuck+ chain: two consecutive tuck+ operations (Fibonacci-like)
\ Stack: 3 5 -> tuck+ -> ( 5 8 ) -> tuck+ -> ( 8 13 )
\ . prints TOS=13, . prints NOS=8
: main 3 5 tuck+ tuck+ . . cr ;
