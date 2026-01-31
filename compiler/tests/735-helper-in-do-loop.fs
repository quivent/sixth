\ expect: 0 1 4 9
\ Test 735: helper called in do loop
: sq dup * ;
: main 4 0 do i sq . loop cr ;
