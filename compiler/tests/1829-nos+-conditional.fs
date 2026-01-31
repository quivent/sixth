\ expect: 99
\ nos+ in conditional: increment NOS and test TOS
\ Stack: 0 1 -> nos+ -> ( 1 1 ) -> if -> takes TOS=1 (true)
\ Inside if: stack has ( 1 ), drop it, push 99
: main 0 1 nos+ if drop 99 else drop 0 then . cr ;
