\ expect: 42
\ STRESS: Multiple CREATE buffers with different sizes - boundary sizes
\ Tests: Creating multiple named buffers, testing 1-byte, 7-byte, 9-byte (misaligned)

create buf1 1 allot
create buf2 7 allot
create buf3 9 allot
create buf4 8 allot

: main
  42 buf4 !     \ aligned 8-byte buffer
  99 buf1 c!    \ 1-byte buffer
  \ Try storing to misaligned buffers
  77 buf2 !     \ 7-byte buffer - will this corrupt buf3?
  88 buf3 !     \ 9-byte buffer - alignment?
  buf4 @        \ should still be 42 (not corrupted)
;
