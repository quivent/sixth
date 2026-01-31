\ Test 805: complex expression chain no intermediate print
\ ((2*3)+(4*5))*(1+1) = (6+20)*2 = 26*2 = 52
: main 2 3 * 4 5 * + 1 1 + * . cr ;
