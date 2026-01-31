\ expect: 1
\ 0<if: branch if TOS < 0, consumes TOS
\ -5 < 0 is true, so takes the if branch
: main -5 0<if 1 else 0 then . cr ;
