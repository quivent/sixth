\ Adversarial test: 2rot word
\ 2rot ( x1 x2 x3 x4 x5 x6 -- x3 x4 x5 x6 x1 x2 )
\ Rotates three double-cell pairs. Bottom pair moves to top.

\ Test 1: Basic 2rot with distinct values
1 2 3 4 5 6 2rot
\ Stack should now be: 3 4 5 6 1 2
2 <> if 1 .err then
1 <> if 2 .err then
6 <> if 3 .err then
5 <> if 4 .err then
4 <> if 5 .err then
3 <> if 6 .err then

\ Test 2: Verify all six cells with different values
10 20 30 40 50 60 2rot
\ Stack should now be: 30 40 50 60 10 20
20 <> if 7 .err then
10 <> if 8 .err then
60 <> if 9 .err then
50 <> if 10 .err then
40 <> if 11 .err then
30 <> if 12 .err then

\ Test 3: 2rot three times should restore original order
100 200 300 400 500 600
2rot 2rot 2rot
\ After three 2rot operations, stack should be back to: 100 200 300 400 500 600
600 <> if 13 .err then
500 <> if 14 .err then
400 <> if 15 .err then
300 <> if 16 .err then
200 <> if 17 .err then
100 <> if 18 .err then

\ Test 4: 2rot with negative values
-1 -2 -3 -4 -5 -6 2rot
\ Stack should now be: -3 -4 -5 -6 -1 -2
-2 <> if 19 .err then
-1 <> if 20 .err then
-6 <> if 21 .err then
-5 <> if 22 .err then
-4 <> if 23 .err then
-3 <> if 24 .err then

\ Test 5: 2rot with mixed positive and negative
1 -1 2 -2 3 -3 2rot
\ Stack should now be: 2 -2 3 -3 1 -1
-1 <> if 25 .err then
1 <> if 26 .err then
-3 <> if 27 .err then
3 <> if 28 .err then
-2 <> if 29 .err then
2 <> if 30 .err then

\ Test 6: 2rot with zeros
0 0 1 1 2 2 2rot
\ Stack should now be: 1 1 2 2 0 0
0 <> if 31 .err then
0 <> if 32 .err then
2 <> if 33 .err then
2 <> if 34 .err then
1 <> if 35 .err then
1 <> if 36 .err then

.ok

\ REMINDER: 2rot rotates three cell pairs. Bottom pair goes to top.
