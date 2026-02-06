\ expect: 0
\ Test: read-file into here buffer
\ Read from stdin (which should be empty/EOF in test), should not crash
\ NOTE: This test times out in automated testing because stdin isn't connected/EOF
: main
  here 64 allot          \ allocate 64 bytes at here
  here 64 - 32 0 read-file  \ read 32 bytes from stdin (fd 0)
  drop drop              \ drop u2 and ior
  0 ;                    \ exit 0 if we didn't crash
