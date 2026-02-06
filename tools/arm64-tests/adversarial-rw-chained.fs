\ expect: ABC
\ Test: multiple write-file calls in sequence
\ Should all work correctly
: main
  s" A" 1 write-file drop
  s" B" 1 write-file drop
  s" C" 1 write-file drop
  0 ;
