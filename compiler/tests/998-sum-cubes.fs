\ Test 998: sum of cubes 1..5 = 1+8+27+64+125 = 225
: cube dup dup * * ;
: main 0 6 1 do i cube + loop . cr ;
