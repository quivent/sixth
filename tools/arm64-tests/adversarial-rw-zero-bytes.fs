\ expect: 0
\ Test: write-file with zero bytes
\ Should succeed (return 0 bytes written, not crash)
: main
  here 0 1 write-file  \ write 0 bytes to stdout
  0= if 0 else 1 then  \ return 0 if ior=0, 1 otherwise
  ;
