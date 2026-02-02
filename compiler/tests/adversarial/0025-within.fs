\ Adversarial test: within ( n low high -- flag )
\ Returns true if low <= n < high (low inclusive, high exclusive)

\ Value inside range (true)
5 3 10 within 1 = assert

\ Value below range (false)
2 3 10 within 0 = assert

\ Value above range (false)
15 3 10 within 0 = assert

\ Value at low boundary (true, inclusive)
3 3 10 within 1 = assert

\ Value at high boundary (false, exclusive)
10 3 10 within 0 = assert

\ Value at high-1 (true)
9 3 10 within 1 = assert

\ Zero inside positive range
0 -5 5 within 1 = assert

\ Negative value in negative range
-3 -10 -1 within 1 = assert

\ Negative value at low boundary
-10 -10 -1 within 1 = assert

\ Negative value at high boundary (exclusive, false)
-1 -10 -1 within 0 = assert

\ Negative value below range
-15 -10 -1 within 0 = assert

\ Negative value above range
5 -10 -1 within 0 = assert

\ Range crossing zero, negative value inside
-2 -5 5 within 1 = assert

\ Range crossing zero, positive value inside
3 -5 5 within 1 = assert

\ Empty range where low = high (always false)
5 5 5 within 0 = assert

\ Empty range where low > high (always false)
5 10 3 within 0 = assert
7 10 3 within 0 = assert
1 10 3 within 0 = assert

\ Single element range [n, n+1)
5 5 6 within 1 = assert
4 5 6 within 0 = assert
6 5 6 within 0 = assert

\ Large positive values
1000 500 1500 within 1 = assert
500 500 1500 within 1 = assert
1499 500 1500 within 1 = assert
1500 500 1500 within 0 = assert

\ REMINDER: within is [low, high). Low inclusive, high exclusive. Classic off-by-one trap.
