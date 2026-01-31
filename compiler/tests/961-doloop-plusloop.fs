\ expect: 0 2 4 6 8
\ Test 961: +loop with step 2 (prints even indices)
: main 10 0 do i . 2 +loop cr ;
