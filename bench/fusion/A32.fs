\ expect: 7 3 7
\ Pattern A32: swap over
\ swap over = tuck — ( 3 7 -- 7 3 7 )
: main 3 7 swap over . . . cr ;
