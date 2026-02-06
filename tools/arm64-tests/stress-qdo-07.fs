\ Stress ?DO test 07: ?DO inside BEGIN-WHILE-REPEAT
\ Test multiple ?DO invocations with decreasing limits
\ n=5: ?DO 0 runs, acc stays 0, n becomes 4
\ n=4: ?DO 0 runs, acc stays 0, n becomes 3
\ n=3: ?DO 0 runs, acc stays 0, n becomes 2
\ etc... until n=0 where while exits
\ Final: acc = 0, but we exit with 5 (initial counter preserved on stack)
\ Actually testing: ?DO correctly handles different limits on each iteration
\ expect: 0
: main 5 begin dup 0> while 0 over 0 ?do i + loop drop 1 - repeat ;
