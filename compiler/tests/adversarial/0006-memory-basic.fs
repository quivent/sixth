\ Adversarial Test 0006: Memory Operations
\ Store. Fetch. Every size.

variable test-var
2variable test-2var
create test-array 10 cells allot

: test-store-fetch ( -- )
  42 test-var ! test-var @ 42 = if ." PASS" else ." FAIL" then cr ;

: test-plus-store ( -- )
  0 test-var ! 5 test-var +! test-var @ 5 = if ." PASS" else ." FAIL" then cr ;

: test-2store-2fetch ( -- )
  1 2 test-2var 2! test-2var 2@ 2 = swap 1 = and if ." PASS" else ." FAIL" then cr ;

: test-c-store-c-fetch ( -- )
  test-array 65 over c! c@ 65 = if ." PASS" else ." FAIL" then cr ;

: test-array-access ( -- )
  10 0 do i test-array i cells + ! loop
  test-array 5 cells + @ 5 = if ." PASS" else ." FAIL" then cr ;

: test-here-comma ( -- )
  here 42 , @ 42 = if ." PASS" else ." FAIL" then cr ;

: test-c-comma ( -- )
  here 65 c, 1- c@ 65 = if ." PASS" else ." FAIL" then cr ;

." 0006-memory-basic: " cr
." ! @:       " test-store-fetch
." +!:        " test-plus-store
." 2! 2@:     " test-2store-2fetch
." c! c@:     " test-c-store-c-fetch
." array:     " test-array-access
." here ,:    " test-here-comma
." c,:        " test-c-comma

\ REMINDER: Memory operations are the foundation. Get these wrong, everything breaks.
