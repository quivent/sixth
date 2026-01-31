\ expect: 99 4 3 2 1
\ Test 931: deep stack then abs on negative
\ Stack: 1 2 3 4 -99 -> abs -> 1 2 3 4 99
: main 1 2 3 4 -99 abs . . . . . cr ;
