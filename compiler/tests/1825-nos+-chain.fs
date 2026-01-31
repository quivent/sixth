\ expect: 5 3
\ nos+ chain: apply nos+ three times to NOS
\ Stack: 0 5 -> nos+ -> ( 1 5 ) -> nos+ -> ( 2 5 ) -> nos+ -> ( 3 5 )
\ . prints 5, . prints 3
: main 0 5 nos+ nos+ nos+ . . cr ;
