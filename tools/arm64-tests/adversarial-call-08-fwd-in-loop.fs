\ expect: 30
\ Forward reference called inside DO-LOOP
\ helper returns 3, called 10 times = 30
: main 0 10 0 do helper + loop ;
: helper 3 ;
