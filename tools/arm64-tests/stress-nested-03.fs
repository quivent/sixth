\ Stress test: BEGIN-UNTIL inside DO-LOOP
\ expect: 10
\ Outer DO-LOOP: 5 iterations (I = 0..4)
\ Inner BEGIN-UNTIL: adds 2 per iteration (loops once, adds 2)
\ 5 * 2 = 10
: main
  0                     \ accumulator
  5 0 do                \ I = 0, 1, 2, 3, 4
    begin
      2 +               \ add 2 to accumulator
      1                 \ true - exit immediately (single iteration)
    until
  loop ;
