\ expect: 1 2 3 4
\ Test 827: do loop with abs on negated values
: main 5 1 do i negate abs . loop cr ;
