\ expect: 99
\ ADVERSARIAL: Zero-length fill (u=0)
\ Tests CBZ X12, done - should skip the loop entirely
\ The pre-existing byte should remain unchanged

: main
  here          \ save address
  99 over c!    \ store 99 at here
  dup 0 65 fill \ fill 0 bytes with 'A' (65)
  c@            \ read byte - should still be 99
;
