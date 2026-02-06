\ Stress ?DO test 08: ?DO with LEAVE
\ LEAVE should exit immediately, setting index=limit
\ Loop 0..10, LEAVE when i=5, count iterations before LEAVE
\ Should count: 0,1,2,3,4,5 then LEAVE (6 iterations counted)
\ expect: 6
: main 0 10 0 ?do 1 + i 5 = if leave then loop ;
