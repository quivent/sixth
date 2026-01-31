\ expect: 1
\ <if: branch if NOS < TOS, consumes both
\ 3 < 5 is true, so takes the if branch
\ NOTE: <if may have register-stack bugs; this test helps verify
: main 3 5 <if 1 else 0 then . cr ;
