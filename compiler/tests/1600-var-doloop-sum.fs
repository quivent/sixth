\ expect: 10
variable v
: main
  0 v !
  5 0 do i v @ + v ! loop
  v @ . cr ;
