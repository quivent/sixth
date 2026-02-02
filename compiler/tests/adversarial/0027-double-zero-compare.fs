\ Test double-cell zero comparison words: d0= and d0<

\ d0= tests - true if double is zero

\ Zero (0 0) should be true
0 0 d0= assert

\ Non-zero low cell (1 0) should be false
1 0 d0= 0= assert

\ Non-zero high cell (0 1) should be false
0 1 d0= 0= assert

\ Both cells non-zero should be false
1 1 d0= 0= assert

\ Negative number (-1 -1) should be false for d0=
-1 -1 d0= 0= assert

\ Large positive should be false
12345 6789 d0= 0= assert

\ d0< tests - true if double is negative (high cell has sign bit set)

\ Positive double should be false
1 0 d0< 0= assert

\ Zero should be false
0 0 d0< 0= assert

\ Negative double (high cell negative) should be true
0 -1 d0< assert

\ -1 as double (-1 -1) should be true (high cell is -1, sign bit set)
-1 -1 d0< assert

\ Large positive (both cells positive) should be false
12345 6789 d0< 0= assert

\ Low cell negative but high cell zero should be false
-1 0 d0< 0= assert

\ Low cell positive but high cell negative should be true
1 -1 d0< assert

\ Minimum negative double should be true
0 -2147483648 d0< assert

\ REMINDER: d0= checks both cells for zero. d0< checks sign bit of high cell.
