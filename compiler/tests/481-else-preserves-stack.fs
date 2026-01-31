\ expect: 42
\ Test 481: if-then preserves stack below (false, no else)
\ Expected output: 42
: main 42 0 if then . cr ;
