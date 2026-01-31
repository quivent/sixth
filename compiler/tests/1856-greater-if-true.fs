\ expect: 1
\ > if: branch if NOS > TOS, consumes both
\ 10 > 3 is true, so takes the if branch
: main 10 3 > if 1 else 0 then . cr ;
