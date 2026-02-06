\ Stress ?DO test 09: ?DO with I and J (nested)
\ Outer ?DO 0..3, Inner ?DO 0..2
\ Sum j*10 + i for all combinations:
\ j=0: i=0,1 -> 0,1 -> sum 1
\ j=1: i=0,1 -> 10,11 -> sum 21
\ j=2: i=0,1 -> 20,21 -> sum 41
\ Total: 1 + 21 + 41 = 63
\ expect: 63
: main 0 3 0 ?do 2 0 ?do j 10 * i + + loop loop ;
