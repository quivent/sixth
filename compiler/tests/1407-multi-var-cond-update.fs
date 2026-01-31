\ expect: 2 2 2
variable a
variable b
variable c
: main
  0 a ! 0 b ! 0 c !
  6 0 do
    i 3 mod 0= if 1 a +! else
    i 3 mod 1 = if 1 b +! else
    1 c +!
    then then
  loop
  a @ . b @ . c @ . cr ;
