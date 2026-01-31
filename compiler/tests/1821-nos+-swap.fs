\ expect: 11 10
\ nos+ then swap to access sum first
\ Stack: 10 10 -> nos+ -> ( 11 10 ) -> swap -> ( 10 11 )
\ . prints 11, . prints 10
: main 10 10 nos+ swap . . cr ;
