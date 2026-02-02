\ expect: 6 7
\ Pattern A05: swap 2*
\ swap then double — wrong register gives 3 14
: main 3 7 swap 2* . . cr ;
