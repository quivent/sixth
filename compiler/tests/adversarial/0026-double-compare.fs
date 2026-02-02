\ Adversarial test: double-cell comparison words d= and d<

\ d= ( d1 d2 -- flag ) compares two double-cell numbers for equality
\ d< ( d1 d2 -- flag ) signed less-than for double-cell numbers

\ ===== d= tests =====

\ Equal positive doubles
1. 1. d= .
\ => -1

\ Equal negative doubles
-1. -1. d= .
\ => -1

\ Unequal doubles (differ in low cell)
1. 2. d= .
\ => 0

\ Unequal doubles (differ in high cell)
0 1 0 2 d= .
\ => 0

\ Zero equals zero
0. 0. d= .
\ => -1

\ Large equal doubles
1000000. 1000000. d= .
\ => -1

\ Mixed: same low cell, different high cell
5 0 5 1 d= .
\ => 0

\ Mixed: same high cell, different low cell
10 5 20 5 d= .
\ => 0

\ ===== d< tests =====

\ Positive d1 < positive d2
1. 2. d< .
\ => -1

\ Negative < positive
-1. 1. d< .
\ => -1

\ Negative < negative (more negative is less)
-10. -5. d< .
\ => -1

\ Equal values (should be false)
5. 5. d< .
\ => 0

\ Large positive numbers
100000. 200000. d< .
\ => -1

\ Larger not less than smaller
200000. 100000. d< .
\ => 0

\ Zero not less than zero
0. 0. d< .
\ => 0

\ Negative not less than more negative
-5. -10. d< .
\ => 0

\ High cell dominates: low cell larger but high cell smaller
0 0 1 1 d< .
\ => -1

\ High cell equal, low cell determines
1 5 2 5 d< .
\ => -1

\ REMINDER: Double comparison uses both cells. High cell dominates for magnitude.
