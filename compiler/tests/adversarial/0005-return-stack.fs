\ Adversarial Test 0005: Return Stack Operations
\ The return stack is where compilers die.

: test-to-r-r-from ( -- )
  5 >r r> 5 = if ." PASS" else ." FAIL" then cr ;

: test-r-fetch ( -- )
  5 >r r@ r> + 10 = if ." PASS" else ." FAIL" then cr ;

: test-multiple-r ( -- )
  3 >r 5 >r r> r> + 8 = if ." PASS" else ." FAIL" then cr ;

: test-r-order ( -- )
  1 >r 2 >r 3 >r r> r> r>
  1 = -rot 2 = swap 3 = and and
  if ." PASS" else ." FAIL" then cr ;

: test-2to-r-2r-from ( -- )
  1 2 2>r 2r> 2 = swap 1 = and if ." PASS" else ." FAIL" then cr ;

: test-2r-fetch ( -- )
  3 4 2>r 2r@ 2r>
  4 = swap 3 = and -rot 4 = swap 3 = and and
  if ." PASS" else ." FAIL" then cr ;

: test-r-in-loop ( -- )
  0
  5 0 do i >r r> + loop
  10 = if ." PASS" else ." FAIL" then cr ;

." 0005-return-stack: " cr
." >r r>:      " test-to-r-r-from
." r@:         " test-r-fetch
." multi r:    " test-multiple-r
." r order:    " test-r-order
." 2>r 2r>:    " test-2to-r-2r-from
." 2r@:        " test-2r-fetch
." r in loop:  " test-r-in-loop

\ REMINDER: Return stack semantics must be exact. No approximations.
