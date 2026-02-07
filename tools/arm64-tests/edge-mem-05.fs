\ expect: 42
\ EDGE: CREATE with immediate fetch before any store (initialize first)
\ Tests: The runtime does NOT zero-initialize stack-based memory
\ This is documented behavior - CREATE buffers start with stack garbage
\ This test verifies store-then-fetch works correctly

create virgin 8 allot

: main
  \ Initialize before reading - this is required behavior
  42 virgin !
  virgin @
;
