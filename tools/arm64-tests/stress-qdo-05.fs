\ Stress ?DO test 05: ?DO with +LOOP positive step
\ index=0, limit=10, step=3: iterations at 0, 3, 6, 9 (4 times)
\ expect: 4
: main 0 10 0 ?do 1 + 3 +loop ;
