\ expect: -5
\ Test: abs then negate in if → -5
: main -5 abs 0 if else negate then . cr ;
