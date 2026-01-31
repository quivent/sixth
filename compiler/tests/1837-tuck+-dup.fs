\ expect: 10 5
\ tuck+ after dup: doubles a value
\ Stack: 5 -> dup -> ( 5 5 ) -> tuck+ -> ( 5 10 )
\ . prints TOS=10, . prints NOS=5
: main 5 dup tuck+ . . cr ;
