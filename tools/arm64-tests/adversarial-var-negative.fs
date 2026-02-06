\ expect: 0
\ Negative values in variables
variable x
: main
  -100 x !
  x @ 100 + ;
