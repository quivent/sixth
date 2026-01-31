\ expect: 50 40 30 20 10
\ Test 920: push 5 values, do/loop, verify values after loop
\ do/loop uses r12/r13. Stack below must survive.
\ 3 0 do loop just runs 3 times doing nothing
: main 10 20 30 40 50 3 0 do loop . . . . . cr ;
