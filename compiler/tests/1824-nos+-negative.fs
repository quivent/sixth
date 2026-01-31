\ expect: 5 -9
\ nos+ with negative NOS
\ Stack: -10 5 -> nos+ -> ( -9 5 ) -> . prints 5 -> . prints -9
: main -10 5 nos+ . . cr ;
