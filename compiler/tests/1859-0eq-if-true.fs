\ expect: 1
\ 0=if: branch if TOS = 0, consumes TOS
\ 0 = 0 is true, so takes the if branch
: main 0 0=if 1 else 0 then . cr ;
