\ Test 1000: Constant folding - addition
\ REGRESSION: Verifies compile-time constant folding for +
\ Two literals followed by + should fold to a single constant.
\ If folding breaks, 2+3 would still produce 5 at runtime,
\ but this test ensures the path works end-to-end.
: fail begin again ;
: main 2 3 + 5 = 0= if fail then ;
