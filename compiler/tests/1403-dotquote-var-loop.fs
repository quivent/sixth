\ expect: x=1 x=2 x=3
variable cnt
: main
  0 cnt !
  3 0 do
    1 cnt +!
    ." x=" cnt @ .
  loop cr ;
