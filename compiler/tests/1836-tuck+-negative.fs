\ expect: -2 -5
\ tuck+ with negative value
\ Stack: 3 -5 -> tuck+ -> ( -5 -2 )
\ . prints TOS=-2, . prints NOS=-5
: main 3 -5 tuck+ . . cr ;
