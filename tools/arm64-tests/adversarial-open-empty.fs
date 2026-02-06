\ expect: 1
\ Test open-file with empty string path
\ Should fail gracefully (return negative fd)
\ Returns 1 if it returns negative fd, 0 if it crashes or returns success
: main s" " 0 open-file drop 0< if 1 else 0 then ;
