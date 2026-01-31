\ expect: 64 32 16 8 4 2 1
\ Test 558: repeated division in while loop (halving)
: main 64 begin dup 1 > while dup . 2 / repeat . cr ;
