\ expect: 120
variable result
: main
  1 result !
  6 1 do i result @ * result ! loop
  result @ . cr ;
