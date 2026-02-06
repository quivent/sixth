\ expect: 255
\ Boundary value - loop that produces max byte value
: main
  0
  255 0 do
    1+
  loop
;
