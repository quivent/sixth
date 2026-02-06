\ expect: 45
\ Test: BEGIN-WHILE-REPEAT inside DO-LOOP inside IF
\ Triple nesting with all three control structures

: main
  0                     \ accumulator
  1 if
    10 0 do
      i                 \ loop counter 0-9
      begin
        dup 0>
      while
        1-
        swap 1+ swap    \ increment accumulator
      repeat
      drop
    loop
  else
    99
  then
;
