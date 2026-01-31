\ Test 970: sort 2 pairs, verify compare-swap with deep stack
\ order: ( a b -- max min ) so . . prints min then max
: order 2dup < if swap then ;
: main 30 10 order . . 5 25 order . . cr ;
