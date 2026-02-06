\ expect: 0
\ Test: write-file from null address (address 0)
\ Should return error or crash gracefully
: main
  0 10 1 write-file   \ write from address 0, length 10, to stdout
  drop 0 ;            \ if we get here, we survived
