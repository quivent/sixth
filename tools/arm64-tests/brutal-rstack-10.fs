\ Brutal Return Stack Test 10: Complex interleaved operations

\ r@ reads TOS of rstack, r> pops it
\ 10 >r 20 >r: rstack = [10, 20] (20 on top)
\ r@ -> 20, r> -> 20, so 20+20=40, r> -> 10, 40+10=50
: t-interleav
  10 >r 20 >r
  r@ r> + r> +
  50 = if ." PASS" else ." FAIL" then cr ;

: t-modify
  5 >r r> 1 + >r r> 6 = if ." PASS" else ." FAIL" then cr ;

: t-shuffle
  1 2 3
  >r swap r>
  3 = -rot 1 = swap 2 = and and
  if ." PASS" else ." FAIL" then cr ;

: t-sequence
  100 >r
  r@ 10 + >r
  r@ 5 + >r
  r> r> r>
  100 = -rot 110 = swap 115 = and and
  if ." PASS" else ." FAIL" then cr ;

: main
  ." brutal-rstack-10: Complex Interleaved Operations" cr
  ." interleaved: " t-interleav
  ." modify:      " t-modify
  ." shuffle:     " t-shuffle
  ." sequence:    " t-sequence
  0 ;
