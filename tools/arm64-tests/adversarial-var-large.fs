\ expect: 1
\ Large value in variable (2^30)
variable big
: main
  1073741824 big !
  big @ 0> if 1 else 0 then ;
