\ expect: 1
\ ADVERSARIAL: High byte values (0x80-0xFF)
\ Tests that LDRB/STRB handle all byte values correctly

variable buf
: main
  here buf ! 20 allot
  buf @
  47 over c!      \ /
  116 over 1+ c!  \ t
  109 over 2 + c! \ m
  112 over 3 + c! \ p
  47 over 4 + c!  \ /
  255 over 5 + c! \ 0xFF
  254 over 6 + c! \ 0xFE
  128 over 7 + c! \ 0x80
  129 over 8 + c! \ 0x81
  9 0 open-file drop drop
  1
;
