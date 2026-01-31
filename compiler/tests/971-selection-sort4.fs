\ Test 971: find min and max of 4 numbers
\ min2 ( a b -- min ): keeps smaller
: min2 ( a b -- c ) 2dup > if swap then drop ;
\ max2 ( a b -- max ): keeps larger
: max2 ( a b -- c ) 2dup < if swap then drop ;
: main 40 10 30 20 min2 min2 min2 . 40 10 30 20 max2 max2 max2 . cr ;
