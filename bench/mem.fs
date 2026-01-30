\ mem.fs - Memory write/read (1K iterations at 8-byte stride)
\ Region: 0x404000-0x406000 (8KB)
: mem ( -- a ) $404000 ;
: main ( -- )
  1000 0 do i i 8 * mem + ! loop
  mem @ . cr ;
