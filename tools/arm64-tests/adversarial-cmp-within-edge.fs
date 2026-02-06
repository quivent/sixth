\ Adversarial: within-style bounds checking edge cases
\ within ( n lo hi -- flag ) is true if lo <= n < hi
\ We implement it manually since within may not be a primitive
\ expect: 1

: my-within ( n lo hi -- flag )
  over -        \ n lo (hi-lo)
  >r - r>       \ (n-lo) (hi-lo)
  u< ;          \ unsigned comparison handles wrap-around

: main
  \ Basic within: 5 in [0,10)
  5 0 10 my-within
  -1 = 0= if 0 exit then

  \ Lower boundary: 0 in [0,10) - should be true
  0 0 10 my-within
  -1 = 0= if 0 exit then

  \ Upper boundary: 10 in [0,10) - should be FALSE (exclusive upper)
  10 0 10 my-within
  0= 0= if 0 exit then

  \ Just below upper: 9 in [0,10)
  9 0 10 my-within
  -1 = 0= if 0 exit then

  \ Below range: -1 in [0,10) - should be false
  -1 0 10 my-within
  0= 0= if 0 exit then

  \ Above range: 11 in [0,10) - should be false
  11 0 10 my-within
  0= 0= if 0 exit then

  \ Negative range: -5 in [-10,0)
  -5 -10 0 my-within
  -1 = 0= if 0 exit then

  \ Empty range: 5 in [5,5) - should be false
  5 5 5 my-within
  0= 0= if 0 exit then

  1 ;
