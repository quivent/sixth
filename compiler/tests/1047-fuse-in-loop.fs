\ Test 1047: Literal-op fusion inside a loop
\ REGRESSION: The actual arith benchmark pattern - fusion must work
\ correctly across loop iterations. 10 iterations of 3*+7 & $FFFFFF.
: main
  1 10 begin dup while
    swap 3 * 7 + $FFFFFF and swap 1-
  repeat drop
  265717 = 0= if begin again then ;
