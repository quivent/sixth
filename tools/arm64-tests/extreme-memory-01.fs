\ expect: 0
\ Test: Large ALLOT with FILL and byte-by-byte verification
\ Allocate 1024 bytes, fill with pattern, verify each byte

variable buf-base
variable ok

: setup-buf here buf-base ! 1024 allot ;

: fill-buf ( -- )
  1024 0 do
    i 255 and buf-base @ i + c!
  loop ;

: check-buf ( -- flag )
  1 ok !
  1024 0 do
    buf-base @ i + c@ i 255 and <> if 0 ok ! then
  loop ok @ ;

: main
  setup-buf
  fill-buf
  check-buf if 0 else 1 then ;
