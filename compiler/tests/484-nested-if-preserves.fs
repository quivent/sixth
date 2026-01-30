\ Test 484: nested if-then preserves stack
\ Expected output: 42
: main 42 1 if 1 if then then . cr ;
