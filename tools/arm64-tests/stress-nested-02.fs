\ Stress test: DO inside BEGIN-WHILE-REPEAT
\ expect: 15
\ Outer: countdown from 5 to 1 using BEGIN-WHILE
\ Inner: DO-LOOP runs 'counter' times, each adding 1
\ When counter=5: loop 5 times, add 5
\ When counter=4: loop 4 times, add 4
\ ... down to counter=1: loop 1 time, add 1
\ Total = 5+4+3+2+1 = 15
: main
  0                     \ accumulator
  5                     \ counter
  begin
    dup 0>              \ while counter > 0
  while
    dup 0 do            \ loop 'counter' times
      swap 1 + swap     \ increment accumulator (under counter)
    loop
    1 -                 \ decrement counter
  repeat
  drop ;                \ drop the 0 counter, leave accumulator
