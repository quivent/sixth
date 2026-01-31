\ expect: 40 50 40 30 20 10
\ Test 914: 5 values, over copies NOS on top
\ Stack: 10 20 30 40 50 -> over -> 10 20 30 40 50 40
\ Print top to bottom: 40 50 40 30 20 10
: main 10 20 30 40 50 over . . . . . . cr ;
