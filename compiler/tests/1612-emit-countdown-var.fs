\ expect: 54321
variable c
: main
  5 c !
  begin c @ 0> while
    c @ 48 + emit
    c @ 1- c !
  repeat cr ;
