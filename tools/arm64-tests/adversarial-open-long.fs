\ expect: 1
\ Test open-file with path near 256 byte limit (255 chars + null = 256)
\ Path buffer is 256 bytes. A 255-char path should fit with null terminator.
\ If the implementation has off-by-one, this might overflow.
\ Returns 1 if fd is negative (file not found), 0 otherwise
: main
  s" /tmp/xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
  0 open-file drop 0< if 1 else 0 then ;
