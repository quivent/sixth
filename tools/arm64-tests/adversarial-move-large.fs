\ expect: OK
\ ADVERSARIAL: Large move (100+ bytes)
\ Tests performance and correctness of byte-by-byte copy for larger blocks
\ Uses a 128-character static string
: main
  s" AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAx"
  drop here 129 move
  \ Verify first byte is 'A' (65)
  here c@ 65 <> if 98 exit then
  \ Verify middle byte is 'A'
  here 64 + c@ 65 <> if 97 exit then
  \ Verify last byte is 'x' (120)
  here 128 + c@ 120 <> if 96 exit then
  ." OK"
;
