\ expect: 0 1
\ nos+ with zero as NOS
\ Stack: 0 0 -> nos+ -> ( 1 0 )
\ . prints TOS=0, . prints 1
: main 0 0 nos+ . . cr ;
