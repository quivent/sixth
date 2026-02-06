\ expect: 1
\ ADVERSARIAL: 300 bytes - definite buffer overflow
\ This overflows the 256-byte path buffer

variable buf
: main
  here buf ! 310 allot
  buf @
  300 0 do 97 over i + c! loop
  300 0 open-file drop drop
  1
;
