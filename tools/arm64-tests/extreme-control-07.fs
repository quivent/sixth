\ expect: 55
\ Test: DO-LOOP with +LOOP and negative step mixed with IF
\ Tests loop control with conditional step modification

: main
  0                     \ sum
  11 1 do
    i +                 \ add i to sum
    i 5 > if
      i
    else
      0
    then
    drop
  loop
;
