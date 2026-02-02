\ adversarial/0032-zero-greater.fs - Test 0> (zero-greater) word
\ 0> ( n -- flag ) returns true if n is greater than zero (positive)

\ Zero is not positive
0 0> 0 = assert

\ Positive numbers return true
1 0> -1 = assert
2 0> -1 = assert
100 0> -1 = assert

\ Negative numbers return false
-1 0> 0 = assert
-2 0> 0 = assert
-100 0> 0 = assert

\ Large positive number
2147483647 0> -1 = assert

\ Large negative number
-2147483648 0> 0 = assert

\ Boundary around zero
1 1 - 0> 0 = assert      \ 0 is not positive
0 1 + 0> -1 = assert     \ 1 is positive
0 1 - 0> 0 = assert      \ -1 is not positive

\ REMINDER: 0> is true only for positive. Zero itself is false.
