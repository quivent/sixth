\ expect: 6 7 5 4 3 2 1
\ Test 913: 7 values on stack, swap top 2, print all
\ Stack: 1 2 3 4 5 6 7 -> swap -> 1 2 3 4 5 7 6
\ Print top to bottom: 6 7 5 4 3 2 1
: main 1 2 3 4 5 6 7 swap . . . . . . . cr ;
