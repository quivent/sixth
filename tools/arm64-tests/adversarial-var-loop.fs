\ expect: 55
\ Variable used as accumulator in loop (sum 1-10)
variable sum
: main
  0 sum !
  10 0 do
    i 1+ sum @ + sum !
  loop
  sum @ ;
