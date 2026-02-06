\ expect: STDOUT
\ Test: write-file to stdout (fd 1)
\ Should successfully write "STDOUT" to standard output
: main
  s" STDOUT" 1 write-file drop  \ write to stdout, drop ior
  0 ;                            \ exit 0
