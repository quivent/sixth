\ expect: 5 6
\ nos+ increments NOS by 1: ( a b -- a+1 b )
\ Stack: 5 5 -> nos+ -> ( 6 5 ), then . prints 5, . prints 6
: main 5 5 nos+ . . cr ;
