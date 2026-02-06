\ Stress ?DO test 06: ?DO with +LOOP negative step
\ index=10, limit=0, step=-2: iterations at 10, 8, 6, 4, 2, 0 (6 times)
\ ANS Forth: exit when crossing from (limit-1) to limit, so body runs at 0
\ Crosses boundary when going from 0 to -2 (crossing through -1 to 0)
\ expect: 6
: main 0 0 10 ?do 1 + -2 +loop ;
