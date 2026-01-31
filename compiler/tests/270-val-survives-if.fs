\ expect: 42
\ Test: value on stack survives empty if → 42
: main 42 1 if then . cr ;
