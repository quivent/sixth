\ expect: 4 7
\ Pattern A01: swap 1+
\ swap then increment — wrong register gives 3 8 or 8 7
: main 3 7 swap 1+ . . cr ;
