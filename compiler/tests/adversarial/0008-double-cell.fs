\ Adversarial Test 0008: Double-Cell Operations
\ 64-bit math on 32-bit cells. Or 128-bit on 64-bit. Compilers hate this.

: test-s-to-d-positive ( -- )
  5 s>d 0 = swap 5 = and if ." PASS" else ." FAIL" then cr ;

: test-s-to-d-negative ( -- )
  -5 s>d -1 = swap -5 = and if ." PASS" else ." FAIL" then cr ;

: test-d-plus ( -- )
  1 0 2 0 d+ 0 = swap 3 = and if ." PASS" else ." FAIL" then cr ;

: test-d-minus ( -- )
  5 0 2 0 d- 0 = swap 3 = and if ." PASS" else ." FAIL" then cr ;

: test-d-negate ( -- )
  5 0 dnegate -1 = swap -5 = and if ." PASS" else ." FAIL" then cr ;

: test-d-abs-positive ( -- )
  5 0 dabs 0 = swap 5 = and if ." PASS" else ." FAIL" then cr ;

: test-d-abs-negative ( -- )
  -5 -1 dabs 0 = swap 5 = and if ." PASS" else ." FAIL" then cr ;

: test-2dup ( -- )
  1 2 2dup 2 = swap 1 = and -rot 2 = swap 1 = and and
  if ." PASS" else ." FAIL" then cr ;

: test-2drop ( -- )
  1 2 3 4 2drop 2 = swap 1 = and if ." PASS" else ." FAIL" then cr ;

: test-2swap ( -- )
  1 2 3 4 2swap
  2 = swap 1 = and -rot 4 = swap 3 = and and
  if ." PASS" else ." FAIL" then cr ;

: test-2over ( -- )
  1 2 3 4 2over
  2 = swap 1 = and -rot 4 = swap 3 = and -rot 2 = swap 1 = and and and
  if ." PASS" else ." FAIL" then cr ;

." 0008-double-cell: " cr
." s>d pos:   " test-s-to-d-positive
." s>d neg:   " test-s-to-d-negative
." d+:        " test-d-plus
." d-:        " test-d-minus
." dnegate:   " test-d-negate
." dabs pos:  " test-d-abs-positive
." dabs neg:  " test-d-abs-negative
." 2dup:      " test-2dup
." 2drop:     " test-2drop
." 2swap:     " test-2swap
." 2over:     " test-2over

\ REMINDER: Double-cell arithmetic. Sign extension matters.
