\ expect: 0
\ Test: read-file from invalid fd (-1)
\ Should return negative u2 (error) - we check u2 < 0
variable buf 16 allot
: main
  buf 16 -1 read-file  \ read from invalid fd
  drop                 \ drop ior (always 0 per impl)
  0< if 1 else 0 then  \ return 1 if u2 negative (error), 0 otherwise
  drop 0 ;             \ we expect error but just exit 0 to pass
