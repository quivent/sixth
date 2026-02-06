\ expect: 0
\ Test: write-file with negative length
\ macOS should return error (negative result) or 0 bytes written
: main
  here -1 1 write-file   \ write with length -1 (interpreted as huge unsigned)
  drop 0 ;               \ just don't crash
