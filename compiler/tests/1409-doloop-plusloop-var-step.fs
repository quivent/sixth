\ expect: 0 3 6 9
variable step
: main
  3 step !
  12 0 do
    i .
  step @ +loop
  cr ;
