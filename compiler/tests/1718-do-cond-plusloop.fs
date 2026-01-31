\ expect: 0 1 3 4 6 7 9
: main
  10 0 do
    i .
    i 3 mod 1 = if 2 else 1 then
  +loop
  cr
;
