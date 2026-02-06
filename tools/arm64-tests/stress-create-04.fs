\ expect: 170
\ STRESS: CREATE with C! and C@ at negative-like offsets (wrap around)
\ Tests: Byte access patterns that might confuse offset calculations

create bytes 16 allot

: main
  \ Fill pattern: 10,20,30,40,70 at offsets 0,1,2,3,4
  10 bytes 0 + c!
  20 bytes 1 + c!
  30 bytes 2 + c!
  40 bytes 3 + c!
  70 bytes 4 + c!
  \ Read back with explicit 0+ to stress offset handling
  bytes 0 + c@        \ 10
  bytes 1 + c@ +      \ +20
  bytes 2 + c@ +      \ +30
  bytes 3 + c@ +      \ +40
  bytes 4 + c@ +      \ +70 = 170
;
