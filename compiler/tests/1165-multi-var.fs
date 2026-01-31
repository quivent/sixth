\ expect: 60
variable x
variable y
variable z
: main
  10 x ! 20 y ! 30 z !
  x @ y @ + z @ + . cr ;
