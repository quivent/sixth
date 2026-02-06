\ expect: 88
\ ADVERSARIAL: Fill same region twice
\ Tests that fill can overwrite previously filled memory
\ Ensures no state leakage between fills

: main
  here 4 allot      \ allocate 4 bytes
  here 4 -          \ start address

  dup 4 65 fill     \ fill with 'A' (65)
  dup 4 88 fill     \ fill with 'X' (88)

  c@                \ read first byte - should be 88
;
