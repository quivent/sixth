\ expected: 190569292
\ Integer partition of 100

: partition ( n max -- count ) recursive
  over 0= if 2drop 1 exit then
  over 0< if 2drop 0 exit then
  dup 0= if 2drop 0 exit then
  2dup - over recurse
  swap 1- recurse + ;

: main
  100 100 partition . cr ;
