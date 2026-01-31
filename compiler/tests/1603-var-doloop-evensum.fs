\ expect: 20
variable total
: main
  0 total !
  10 0 do
    i 2 mod 0= if i total @ + total ! then
  loop
  total @ . cr ;
