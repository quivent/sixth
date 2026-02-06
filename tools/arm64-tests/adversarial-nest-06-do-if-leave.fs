\ Adversarial test: DO-LOOP with IF inside and LEAVE
\ Tests early exit from loop with conditional
\ expect: 6

: main
  0           \ accumulator
  10 0 do
    i +       \ add index to accumulator
    i 3 = if leave then  \ exit when i=3
  loop
;
\ i=0: acc=0
\ i=1: acc=1
\ i=2: acc=3
\ i=3: acc=6, then leave
\ Result: 6
