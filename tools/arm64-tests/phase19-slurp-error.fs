\ expect: 1
\ Test slurp-file: verify error handling for non-existent file

: main
  s" /nonexistent/file/that/does/not/exist.txt" slurp-file  ( addr u ior )
  0< if                  \ ior < 0 means error
    2drop 1              \ success: error was detected
  else
    2drop 0              \ fail: should have returned error
  then
;
