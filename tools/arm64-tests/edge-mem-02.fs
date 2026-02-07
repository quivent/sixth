\ expect: 123
\ EDGE: Multiple CREATE words with overlapping usage patterns
\ Tests: Define 5 buffers, write to all in different order than declared,
\ then read back to verify no memory aliasing or corruption
\ This stresses the variable offset tracking in the compiler

create buf1 8 allot
create buf2 16 allot
create buf3 8 allot
create buf4 24 allot
create buf5 8 allot

: main
  \ Write to buffers in reverse order of declaration
  55 buf5 !
  44 buf4 !
  33 buf3 !
  22 buf2 !
  11 buf1 !

  \ Write second cell to multi-cell buffers (if size allows)
  66 buf2 8 + !
  77 buf4 8 + !
  88 buf4 16 + !

  \ Verify original cells weren't corrupted
  buf1 @           \ 11
  buf2 @           \ 22
  +                \ 33
  buf3 @           \ 33
  +                \ 66
  buf4 @           \ 44
  +                \ 110
  buf5 @           \ 55
  +                \ 165

  \ Verify second cells are correct
  buf2 8 + @       \ 66
  +                \ 231
  buf4 8 + @       \ 77
  +                \ 308
  buf4 16 + @      \ 88
  +                \ 396

  \ Reduce to fit in exit code
  273 -            \ 396 - 273 = 123
;
