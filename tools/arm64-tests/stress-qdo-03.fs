\ Stress ?DO test 03: start > limit (should skip)
\ ?DO with index=10, limit=5 should not execute at all
\ expect: 99
: main 99 5 10 ?do drop 0 loop ;
