\ expect: 1
\ nos+ then compare: after nos+ ( a+1 b ), check if equal
\ Stack: 4 5 -> nos+ -> ( 5 5 ) -> = -> ( -1 true )
\ But = in this Forth returns -1 for true
: main 4 5 nos+ = if 1 else 0 then . cr ;
