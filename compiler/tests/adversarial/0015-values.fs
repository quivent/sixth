\ Adversarial Test 0015: Values and TO
\ Local-like behavior without locals.

5 value my-value

: test-value-fetch ( -- )
  my-value 5 = if ." PASS" else ." FAIL" then cr ;

: test-value-to ( -- )
  10 to my-value
  my-value 10 = if ." PASS" else ." FAIL" then cr ;

: test-value-in-colon ( -- )
  my-value 1+ to my-value
  my-value 11 = if ." PASS" else ." FAIL" then cr ;

0 0 2value my-2value

: test-2value-fetch ( -- )
  1 2 to my-2value
  my-2value 2 = swap 1 = and if ." PASS" else ." FAIL" then cr ;

: test-2value-to ( -- )
  3 4 to my-2value
  my-2value 4 = swap 3 = and if ." PASS" else ." FAIL" then cr ;

." 0015-values: " cr
." value @:   " test-value-fetch
." value to:  " test-value-to
." value def: " test-value-in-colon
." 2value @:  " test-2value-fetch
." 2value to: " test-2value-to

\ REMINDER: VALUES look like constants but are mutable. Confusing but useful.
