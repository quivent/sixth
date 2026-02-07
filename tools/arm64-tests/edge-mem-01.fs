\ expect: 170
\ EDGE: Large ALLOT (1024+ bytes) followed by store/fetch at boundaries
\ Tests: Allocating 1024 bytes, writing/reading at offset 0, 512, 1016 (last cell)
\ This tests the here pointer handling for larger allocations
\ and verifies boundary accesses don't corrupt adjacent memory

create bigbuf 1024 allot

: main
  \ Store at the very beginning (offset 0)
  42 bigbuf !

  \ Store in the middle (offset 512)
  77 bigbuf 512 + !

  \ Store at the end (offset 1016 = last aligned 8-byte cell in 1024 bytes)
  99 bigbuf 1016 + !

  \ Read back and check all three are intact
  \ If memory layout is corrupted, these will fail
  bigbuf @                     \ should be 42
  bigbuf 512 + @               \ should be 77
  +                            \ 42 + 77 = 119
  bigbuf 1016 + @              \ should be 99
  +                            \ 119 + 99 = 218

  \ Subtract a magic value to get a small testable result
  48 -                         \ 218 - 48 = 170
;
