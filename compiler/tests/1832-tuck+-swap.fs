\ expect: 2 3
\ tuck+ then swap and print
\ Stack: 1 2 -> tuck+ -> ( 2 3 ) -> swap -> ( 3 2 )
\ . prints TOS=2, . prints NOS=3
: main 1 2 tuck+ swap . . cr ;
