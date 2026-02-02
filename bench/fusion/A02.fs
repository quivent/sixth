\ expect: 2 7
\ Pattern A02: swap 1-
\ swap then decrement — wrong register gives 3 6 or 6 7
: main 3 7 swap 1- . . cr ;
