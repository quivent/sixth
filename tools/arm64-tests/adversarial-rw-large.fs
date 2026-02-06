\ expect: 0
\ Test: write-file with larger buffer (256 bytes)
\ Should handle non-trivial buffer size without crashing
variable bigbuf 256 allot
: main
  65 bigbuf c!                 \ 'A' at start
  66 bigbuf 255 + c!           \ 'B' at end
  bigbuf 256 1 write-file     \ write to stdout
  drop 0 ;                     \ ignore ior, exit 0
