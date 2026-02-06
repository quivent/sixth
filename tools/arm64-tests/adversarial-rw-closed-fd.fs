\ expect: 0
\ Test: write-file to closed file descriptor
\ Open /dev/null, close it, then try to write to the closed fd
\ Should return error (negative or positive ior)
: main
  s" /dev/null" 0 open-file drop  \ open /dev/null, drop ior, keep fd
  dup close-file drop             \ close it, drop ior
  \ fd is still on stack but now closed
  s" test" rot write-file         \ ( addr u fd -- ior )
  0<> if 0 else 1 then            \ return 0 if got error, 1 if no error
  ;
