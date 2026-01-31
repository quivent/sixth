\ Test 968: compute in loop, test result with if
: sumto 0 swap 1+ 1 do i + loop ;
: main 5 sumto dup 15 = if . else drop 0 . then cr ;
