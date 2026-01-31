\ expect: 16
\ Test: swap-based accumulator doubling per iteration → 16
: main 1 5 begin dup 1 > while swap 2* swap 1- repeat drop . cr ;
