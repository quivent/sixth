\ expect: 7 0
\ tuck+ with zero as TOS: ( 7 0 -- 0 7 )
\ Stack: 7 0 -> tuck+ -> ( 0 7 )
\ . prints TOS=7, . prints NOS=0
: main 7 0 tuck+ . . cr ;
