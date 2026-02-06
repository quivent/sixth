\ Stress test: Deep nesting (5 levels) - IF/DO/BEGIN/IF/DO
\ expect: 8
\ Level 1: IF (true)
\ Level 2: DO-LOOP (2 iter)
\ Level 3: BEGIN-UNTIL (2 iter)
\ Level 4: IF (true)
\ Level 5: DO-LOOP (2 iter)
\ Total innermost = 2 * 2 * 2 = 8
: main
  0                         \ accumulator
  1 if                      \ L1: always true
    2 0 do                  \ L2: 2 iterations (I=0,1)
      0                     \ counter for until
      begin
        1 if                \ L4: always true
          2 0 do            \ L5: 2 iterations
            swap 1 + swap   \ increment accumulator (under counter)
          loop
        then
        1 +                 \ increment until-counter
        dup 2 =             \ exit when counter reaches 2
      until
      drop                  \ drop counter
    loop
  then ;
