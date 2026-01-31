\ expect: 6
\ Test: nested while loops, accumulate inner iterations → 6
: main 0 3 begin dup 0 > while swap 2 + swap 1- repeat drop . cr ;
