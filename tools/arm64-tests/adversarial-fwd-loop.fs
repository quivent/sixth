\ expect: 10
\ Forward reference called in loop
: main 0 5 0 do helper + loop ;
: helper 2 ;
