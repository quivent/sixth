\ expect: 77
\ ADVERSARIAL: Fill multiple regions, verify no bleed
\ Tests that fill operations don't corrupt adjacent memory
\ Fill region A with X, region B with Y, verify boundaries

: main
  here                    \ base
  16 allot                \ allocate 16 bytes
  here 16 -               \ back to start
  dup 4 65 fill           \ first 4 bytes = 'A' (65)
  dup 4 + 4 66 fill       \ next 4 bytes = 'B' (66)
  dup 8 + 4 67 fill       \ next 4 bytes = 'C' (67)
  dup 12 + 4 68 fill      \ last 4 bytes = 'D' (68)
  \ Verify boundaries: byte 3 should be 65, byte 4 should be 66
  dup 3 + c@ 65 =         \ byte 3 = 65?
  over 4 + c@ 66 = and    \ byte 4 = 66?
  if 2drop 77 else 2drop 0 then  \ 77 if correct
;
