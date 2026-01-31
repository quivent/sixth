\ expect: 20 10
variable a
variable b
: main
  10 a ! 20 b !
  a @ b @ a ! b !
  a @ . b @ . cr ;
