\ expect: 1
\ = if: branch if NOS = TOS, consumes both
\ 7 = 7 is true, so takes the if branch
: main 7 7 = if 1 else 0 then . cr ;
