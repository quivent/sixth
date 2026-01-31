\ expect: 9
\ Test 983: digital root of 9999 = 9
\ Digital root: keep summing digits until single digit
: dsum 0 swap begin dup while dup 10 mod rot + swap 10 / repeat drop ;
: droot begin dup 10 < if exit then dsum again ;
: main 9999 droot . cr ;
