\ Adversarial Test 0009: Mixed-Precision Arithmetic
\ Star-slash. UM operations. The subtle ones.

: test-star-slash ( -- )
  10 3 2 */ 15 = if ." PASS" else ." FAIL" then cr ;

: test-star-slash-mod ( -- )
  10 3 2 */mod 1 = swap 15 = and if ." PASS" else ." FAIL" then cr ;

: test-um-star ( -- )
  3 4 um* 0 = swap 12 = and if ." PASS" else ." FAIL" then cr ;

: test-um-div-mod ( -- )
  0 10 3 um/mod 3 = swap 1 = and if ." PASS" else ." FAIL" then cr ;

: test-m-star ( -- )
  -3 4 m* -1 = swap -12 = and if ." PASS" else ." FAIL" then cr ;

: test-sm-div-rem ( -- )
  -7 0 d>s 3 sm/rem -2 = swap -1 = and if ." PASS" else ." FAIL" then cr ;

: test-fm-div-mod ( -- )
  -7 0 d>s 3 fm/mod -3 = swap 2 = and if ." PASS" else ." FAIL" then cr ;

: test-max ( -- )
  3 5 max 5 = if ." PASS" else ." FAIL" then cr ;

: test-min ( -- )
  3 5 min 3 = if ." PASS" else ." FAIL" then cr ;

: test-max-negative ( -- )
  -3 -5 max -3 = if ." PASS" else ." FAIL" then cr ;

: test-min-negative ( -- )
  -3 -5 min -5 = if ." PASS" else ." FAIL" then cr ;

." 0009-mixed-math: " cr
." */:        " test-star-slash
." */mod:     " test-star-slash-mod
." um*:       " test-um-star
." um/mod:    " test-um-div-mod
." m*:        " test-m-star
." max:       " test-max
." min:       " test-min
." max neg:   " test-max-negative
." min neg:   " test-min-negative

\ REMINDER: Mixed precision is where overflow bugs hide.
