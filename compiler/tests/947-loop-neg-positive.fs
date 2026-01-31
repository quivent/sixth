\ Test 947: loop counter goes negative then back positive
\ Start at -2, count up to 2: prints -2 -1 0 1
: main 2 -2 do i . loop cr ;
