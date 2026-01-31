\ Test 1049: Folding must not leak onto runtime stack
\ REGRESSION: Pure constant expressions that are folded should produce
\ exactly one value on the stack, not multiple intermediate values.
: main 2 3 + 4 * 20 = 0= if begin again then ;
