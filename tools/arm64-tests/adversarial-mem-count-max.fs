\ expect: 200
\ ADVERSARIAL: Count with maximum byte length (255)
\ Tests that count correctly handles max 8-bit length
\ Verifies no sign extension or truncation issues

: main
  here                    \ address for counted string
  255 over c!             \ store length 255
  count                   \ ( addr+1 len )
  nip                     \ just keep length (should be 255)
  255 = if 200 else 0 then  \ return 200 if correct
;
