\ expect: 20
\ Sum even numbers 0..9: 0+2+4+6+8 = 20
variable sum
: main
  0 sum !
  10 0 do
    i 2 mod 0= if
      i sum @ + sum !
    then
  loop
  sum @ . cr ;
