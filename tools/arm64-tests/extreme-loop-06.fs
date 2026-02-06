\ expect: 210
\ Large iteration count - 21 iterations, sum of 0..20
\ Tests for off-by-one in loop termination
: main
  0
  21 0 do
    i +
  loop
;
\ 0+1+2+...+20 = 20*21/2 = 210
