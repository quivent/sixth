\ expect: STDERR
\ Test: write-file to stderr (fd 2)
\ Should successfully write "STDERR" to standard error
: main
  s" STDERR" 2 write-file drop  \ write to stderr, drop ior
  0 ;                            \ exit 0
