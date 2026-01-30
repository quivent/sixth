\ Test: accumulate sum 1+2+3+4+5 in loop → 15
: main 0 5 begin dup 0 > while swap over + swap 1- repeat drop . cr ;
