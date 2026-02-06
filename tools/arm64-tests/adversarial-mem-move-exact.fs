\ expect: 65
\ ADVERSARIAL: Move with exact byte count (boundary precision)
\ Tests that move copies exactly N bytes, no more, no less
\ Copy 3 bytes "ABC", verify byte 4 is unchanged

: main
  here 8 allot            \ allocate target buffer
  here 8 -                \ target base
  dup 8 0 fill            \ clear buffer to 0
  s" ABCD" drop           \ source address
  over                    \ target address
  3 move                  \ copy exactly 3 bytes (ABC)
  3 + c@                  \ read byte 4 (should be 0, not 'D')
  0= if 65 else 0 then    \ if 0, return 65 (success)
;
