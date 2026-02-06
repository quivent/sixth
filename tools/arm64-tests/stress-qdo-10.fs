\ Stress ?DO test 10: multiple ?DO in same word
\ First ?DO: 0..3 (3 iters), sum I = 0+1+2 = 3
\ Second ?DO: 0..0 (0 iters), adds nothing
\ Third ?DO: 5..10 (5 iters), count = 5
\ Result: 3 + 0 + 5 = 8
\ expect: 8
: main 0 3 0 ?do i + loop 0 0 ?do 100 + loop 10 5 ?do 1 + loop ;
