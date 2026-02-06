\ expect: 1
\ Test multiple open-file calls in sequence
\ Each call uses the same path buffer - check for corruption
\ Open /dev/null 3 times, should all succeed
: try-open ( -- flag ) s" /dev/null" 0 open-file drop 0>= ;
: main
  try-open 0= if 0 exit then
  try-open 0= if 0 exit then
  try-open 0= if 0 exit then
  1 ;
