\ expect: 15
\ Variable modified multiple times - should hold final value
variable x
: main
  5 x !
  10 x !
  x @ 5 + x !
  x @ ;
