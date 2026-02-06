\ expect: 0
\ Test: write-file to very high file descriptor
\ fd 999 should definitely be invalid, return error
: main
  s" test" 999 write-file  \ write to fd 999
  0< if 0 else 0 then      \ negative = error (expected), return 0 either way
  ;
