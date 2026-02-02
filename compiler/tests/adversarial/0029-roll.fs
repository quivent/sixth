\ Test: roll word
\ roll ( xu xu-1 ... x0 u -- xu-1 ... x0 xu ) rotates u+1 items, bringing xu to top

\ Test 0 roll - does nothing
10 0 roll
10 = if 1 . then

\ Test 1 roll - same as swap
20 30 1 roll
20 = if 2 . then
30 = if 3 . then

\ Test 2 roll - same as rot
40 50 60 2 roll
50 = if 4 . then
60 = if 5 . then
40 = if 6 . then

\ Test 3 roll - four items rotated
100 200 300 400 3 roll
200 = if 7 . then
300 = if 8 . then
400 = if 9 . then
100 = if 10 . then

\ Test with different values to verify correct item is brought up
1 2 3 4 5 4 roll
\ Stack should be: 2 3 4 5 1
1 = if 11 . then
5 = if 12 . then
4 = if 13 . then
3 = if 14 . then
2 = if 15 . then

\ Verify 1 roll matches swap behavior
77 88 1 roll
77 = if 16 . then
88 = if 17 . then

\ Verify 2 roll matches rot behavior
111 222 333 2 roll
222 = if 18 . then
333 = if 19 . then
111 = if 20 . then

cr
bye

\ Expected output: 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19 20

\ REMINDER: roll moves item to top, shifting others down. 1 roll = swap. 2 roll = rot.
