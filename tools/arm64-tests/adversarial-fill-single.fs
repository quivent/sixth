\ expect: 65
\ ADVERSARIAL: Single byte fill
\ Tests the loop executes exactly once and terminates correctly
\ This catches off-by-one errors in the loop condition

: main
  here          \ save address
  0 over c!     \ clear byte at here
  dup 1 65 fill \ fill 1 byte with 'A' (65)
  c@            \ read byte - should be 65
;
