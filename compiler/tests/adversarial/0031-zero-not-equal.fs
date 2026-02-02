\ Test 0<> (zero-not-equal) word
\ 0<> ( n -- flag ) returns true if n is not zero, false if n is zero

\ Zero should return false
0 0<> 0 = assert

\ One should return true
1 0<> 0 <> assert

\ Negative one should return true
-1 0<> 0 <> assert

\ Small positive numbers should return true
2 0<> 0 <> assert
5 0<> 0 <> assert
10 0<> 0 <> assert

\ Small negative numbers should return true
-2 0<> 0 <> assert
-5 0<> 0 <> assert
-10 0<> 0 <> assert

\ Large positive number should return true
1000000 0<> 0 <> assert

\ Large negative number should return true
-1000000 0<> 0 <> assert

\ Maximum positive value should return true
2147483647 0<> 0 <> assert

\ Minimum negative value should return true
-2147483648 0<> 0 <> assert

\ Verify 0<> is inverse of 0=
0 0<> 0 0= invert = assert
1 0<> 1 0= invert = assert
-1 0<> -1 0= invert = assert
42 0<> 42 0= invert = assert

\ REMINDER: 0<> is true for any non-zero. Inverse of 0=.
