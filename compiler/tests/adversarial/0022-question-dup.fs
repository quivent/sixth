\ Adversarial test: ?dup (conditional duplicate)
\ ?dup duplicates top of stack only if non-zero; leaves zero alone

\ Test 1: ?dup with non-zero positive value (should duplicate)
5 ?dup
2 pick 5 <> if 1 .err then
dup 5 <> if 2 .err then
2drop

\ Test 2: ?dup with zero (should NOT duplicate, stack unchanged)
0 ?dup
dup 0 <> if 3 .err then
drop

\ Test 3: ?dup with negative value (should duplicate, it's non-zero)
-7 ?dup
2 pick -7 <> if 4 .err then
dup -7 <> if 5 .err then
2drop

\ Test 4: ?dup with -1 (true flag, should duplicate)
-1 ?dup
2 pick -1 <> if 6 .err then
dup -1 <> if 7 .err then
2drop

\ Test 5: ?dup with 1 (should duplicate)
1 ?dup
2 pick 1 <> if 8 .err then
dup 1 <> if 9 .err then
2drop

\ Test 6: Verify stack depth after ?dup with zero
depth
0 ?dup drop
depth swap - 0 <> if 10 .err then

\ Test 7: Verify stack depth after ?dup with non-zero
depth
42 ?dup 2drop
depth swap - 0 <> if 11 .err then

\ Test 8: ?dup with large positive value
999999 ?dup
+ 1999998 <> if 12 .err then

\ Test 9: ?dup with large negative value
-999999 ?dup
+ -1999998 <> if 13 .err then

\ Test 10: Sequential ?dup operations
3 ?dup ?dup
\ Stack should be: 3 3 3
+ + 9 <> if 14 .err then

\ Test 11: ?dup zero does not affect values below
100 0 ?dup
drop 100 <> if 15 .err then

\ Test 12: ?dup non-zero preserves values below
100 50 ?dup
+ 100 <> if 16 .err then
100 <> if 17 .err then

\ REMINDER: ?dup duplicates non-zero, leaves zero alone. Simple but essential.
