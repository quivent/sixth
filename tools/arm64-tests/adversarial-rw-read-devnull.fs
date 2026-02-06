\ expect: 0
\ Test: read-file from /dev/null
\ Should return 0 bytes read (EOF immediately)
variable rbuf 64 allot
: main
  s" /dev/null" 0 open-file drop  \ open /dev/null, keep fd
  rbuf 64 rot read-file           \ read into buffer
  drop                            \ drop ior
  0= if 0 else 1 then             \ expect 0 bytes read
  ;
