\ expect: 231
\ ADVERSARIAL: Large allocation + fill + read at various offsets
\ Tests that fill works correctly over larger buffers
\ Return value: sum of first, middle, and last bytes (77+77+77=231)
: main
  here           \ save start
  256 allot      \ allocate 256 bytes
  dup 256 77 fill   \ fill with 'M' (77)
  \ Read first, middle, and last bytes and sum them
  dup c@            \ byte 0 = 77
  over 128 + c@ +   \ + byte 128 = 77 -> 154
  swap 255 + c@ +   \ + byte 255 = 77 -> 231
;
