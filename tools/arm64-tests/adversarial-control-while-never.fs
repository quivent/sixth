\ Adversarial control flow: while condition false immediately
\ Tests loop body never executes
\ expect: 88
: main 88 begin 0 while drop 99 repeat ;
