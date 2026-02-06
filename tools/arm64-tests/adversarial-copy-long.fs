\ expect: 1
\ ADVERSARIAL: Exactly 255 byte path
\ Tests near-maximum path length (buffer is 256, need 1 for null)

variable buf
: main
  here buf ! 260 allot
  buf @
  47 over c!  116 over 1+ c!  109 over 2 + c!  112 over 3 + c!  47 over 4 + c!
  250 0 do 97 over 5 i + + c! loop
  255 0 open-file drop drop
  1
;
