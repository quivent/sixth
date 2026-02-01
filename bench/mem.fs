\ mem.fs - Memory write/read (100K passes over 1K entries)
\ Region: 0x404000-0x406000 (8KB)
: mem ( -- a ) $404000 ;
: main ( -- )
  100000 0 do
    1000 0 do i i 8 * mem + ! loop
  loop
  mem @ . cr ;
