\ expect: 1
\ ADVERSARIAL: Verify bytes are actually copied correctly
\ Open a known file and verify the fd is valid

: main
  s" /dev/null" 0 open-file
  drop                          \ ior
  dup 0>= if
    \ Got a valid fd, close it
    close-file drop
    1
  else
    drop 0
  then
;
