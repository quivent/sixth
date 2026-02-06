\ expect: 1
\ ADVERSARIAL: Exactly 256 bytes - AT buffer limit
\ Buffer is 256 bytes. 256 data + 1 null = potential overflow

variable buf
: main
  here buf ! 260 allot
  buf @
  256 0 do 97 over i + c! loop
  256 0 open-file drop drop
  1
;
