\ adversarial/0033-du-less.fs - Test du< (double unsigned less-than)

\ du< ( ud1 ud2 -- flag ) unsigned less-than for double-cell numbers

\ Small ud1 < large ud2 (true)
1. 100. du< .   \ 1 (1 < 100)
0. 1. du< .     \ 1 (0 < 1)
50. 51. du< .   \ 1 (50 < 51)

\ Large ud1 > small ud2 (false)
100. 1. du< .   \ 0 (100 > 1)
1000. 500. du< .  \ 0 (1000 > 500)

\ Equal values (false)
0. 0. du< .     \ 0 (equal)
1. 1. du< .     \ 0 (equal)
12345. 12345. du< .  \ 0 (equal)

\ Large doubles where high cell matters
1 0 2 0 du< .   \ 1 (high cells: 0 < 0? no, low cells: 1 < 2)
0 1 0 2 du< .   \ 1 (high cell 1 < 2)
0 2 0 1 du< .   \ 0 (high cell 2 > 1)
0 1 0 1 du< .   \ 0 (equal)

\ High cell equal, low cell differs
5 10 6 10 du< .   \ 1 (high equal, 5 < 6)
6 10 5 10 du< .   \ 0 (high equal, 6 > 5)

\ Values that would differ if treated as signed
\ -1 as unsigned is max value (all bits set)
-1. 0. du< .    \ 0 (max unsigned > 0)
0. -1. du< .    \ 1 (0 < max unsigned)
-1. 1. du< .    \ 0 (max unsigned > 1)
1. -1. du< .    \ 1 (1 < max unsigned)

\ High cell has sign bit set (would be negative if signed)
0 -1 0 1 du< .  \ 0 (unsigned: huge > small)
0 1 0 -1 du< .  \ 1 (unsigned: small < huge)

\ Maximum unsigned double vs smaller value
-1 -1 0 0 du< . \ 0 (max > 0)
0 0 -1 -1 du< . \ 1 (0 < max)
-1 -1 -1 -2 du< .  \ 0 (high: -1 vs -2 unsigned, -1 > -2 as unsigned)
-1 -2 -1 -1 du< .  \ 1 (high cells equal, low: -2 < -1 unsigned)

\ Boundary: just below vs at boundary
-2 -1 -1 -1 du< .  \ 1 (one less than max < max)
-1 -1 -2 -1 du< .  \ 0 (max > one less)

\ Zero comparisons
0. 0. du< .     \ 0
0 0 0 1 du< .   \ 1 (0 < large)
0 1 0 0 du< .   \ 0 (large > 0)

\ REMINDER: du< treats double as unsigned. All 64 bits are magnitude.
