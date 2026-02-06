\ Stress ?DO test 02: negative start and limit
\ ?DO with index=-5, limit=-2 should iterate 3 times (-5,-4,-3)
\ Sum: -5 + -4 + -3 = -12, but we use absolute values
\ Actually, just count iterations: start at 0, add 1 each time
\ expect: 3
: main 0 -2 -5 ?do 1 + loop ;
