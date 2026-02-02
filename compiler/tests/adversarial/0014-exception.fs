\ Adversarial Test 0014: Exception Handling
\ CATCH and THROW. Error propagation.

: thrower ( n -- ) throw ;

: test-catch-no-throw ( -- )
  ['] noop catch 0= if ." PASS" else ." FAIL" then cr ;

: test-catch-throw ( -- )
  ['] thrower catch -1 = if ." PASS" else ." FAIL" then cr ;

-1 ' thrower catch drop

: test-throw-zero ( -- )
  0 throw ." PASS" cr ;

: nested-throw ( -- ) -2 throw ;
: outer-catch ( -- n ) ['] nested-throw catch ;

: test-nested-catch ( -- )
  outer-catch -2 = if ." PASS" else ." FAIL" then cr ;

: abort-test ( flag -- )
  abort" test abort" ;

: test-abort-false ( -- )
  ['] abort-test catch 0= if ." PASS" else ." FAIL" then cr ;

false ' abort-test catch drop

." 0014-exception: " cr
." catch ok:  " test-catch-no-throw
." catch thr: " test-catch-throw
." throw 0:   " test-throw-zero
." nested:    " test-nested-catch
." abort f:   " test-abort-false

\ REMINDER: Exceptions unwind the stack. Both stacks. Get it wrong, memory corruption.
