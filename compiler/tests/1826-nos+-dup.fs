\ expect: 3 4
\ nos+ combined with dup
\ Stack: 3 -> dup -> ( 3 3 ) -> nos+ -> ( 4 3 )
\ . prints TOS=3, . prints 4
: main 3 dup nos+ . . cr ;
