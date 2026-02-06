\ expect: 255
\ ADVERSARIAL: Fill with 0xFF (max byte value)
\ Tests that the full byte range is handled correctly
\ 255 tests sign extension issues if any

: main
  here          \ save address
  0 over c!     \ clear byte
  dup 1 255 fill \ fill 1 byte with 255
  c@            \ read byte - should be 255
;
