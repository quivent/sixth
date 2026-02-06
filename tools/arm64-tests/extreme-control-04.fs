\ expect: 20
\ Test: LEAVE from deeply nested loops
\ Inner LEAVE must exit only innermost loop

: main
  0                     \ sum
  5 0 do
    10 0 do
      i 3 = if
        leave           \ exit inner loop when i=3
      then
      1+                \ count iterations
    loop
  loop
;
