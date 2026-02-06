\ Stress ?DO test 04: nested ?DO loops
\ Outer: 0..2 (3 iters), Inner: 0..1 (2 iters each)
\ Inner runs only when j < inner limit
\ Count total iterations: 3 * 2 = 6
\ expect: 6
: main 0 3 0 ?do 2 0 ?do 1 + loop loop ;
