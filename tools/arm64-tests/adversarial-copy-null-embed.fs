\ expect: 1
\ ADVERSARIAL: String with embedded null byte
\ Tests that the copy loop copies ALL bytes, not stopping at null

variable buf
: main
  here buf ! 16 allot
  buf @
  47 over c!        \ /
  116 over 1+ c!    \ t
  109 over 2 + c!   \ m
  112 over 3 + c!   \ p
  0 over 4 + c!     \ null byte (embedded)
  120 over 5 + c!   \ x (after null)
  6 0 open-file drop drop
  1
;
