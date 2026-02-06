\ expect: 1
\ Test open-file with /dev/null (should succeed)
\ Returns 1 if fd >= 0, 0 otherwise
: main s" /dev/null" 0 open-file drop 0>= if 1 else 0 then ;
