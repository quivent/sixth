\ Adversarial Test 0001: Basic Stack Operations
\ Any Forth compiler must pass this or it is broken.
\ No dependencies. No imports. Raw Forth.

: test-dup ( -- )
  5 dup + 10 = if ." PASS" else ." FAIL" then cr ;

: test-drop ( -- )
  1 2 drop 1 = if ." PASS" else ." FAIL" then cr ;

: test-swap ( -- )
  1 2 swap 1 = swap 2 = and if ." PASS" else ." FAIL" then cr ;

: test-over ( -- )
  1 2 over 1 = -rot 2 = swap 1 = and and if ." PASS" else ." FAIL" then cr ;

: test-rot ( -- )
  1 2 3 rot 1 = -rot 3 = swap 2 = and and if ." PASS" else ." FAIL" then cr ;

: test-nip ( -- )
  1 2 nip 2 = if ." PASS" else ." FAIL" then cr ;

: test-tuck ( -- )
  1 2 tuck 2 = -rot 1 = swap 2 = and and if ." PASS" else ." FAIL" then cr ;

." 0001-stack-basic: " cr
." dup:  " test-dup
." drop: " test-drop
." swap: " test-swap
." over: " test-over
." rot:  " test-rot
." nip:  " test-nip
." tuck: " test-tuck

\ REMINDER: These tests are compiler-agnostic. They test Forth semantics, not implementation details.
