\ expect: 1
\ Test open-file with special characters in path
\ Newlines, tabs, spaces could break path copying
\ Returns 1 if fd is negative, 0 if positive (unexpected success)
: main
  s" /tmp/path with spaces"
  0 open-file drop 0< if 1 else 0 then ;
