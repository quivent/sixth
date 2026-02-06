\ Adversarial test: DO-LOOP containing BEGIN-WHILE-REPEAT
\ Tests mixing counted and conditional loops
\ expect: 6

: main
  0           \ accumulator
  3 0 do      \ i = 0,1,2
    i 1+      \ inner counter = i+1
    begin
      dup 0 >
    while
      swap 1+ swap  \ increment accumulator
      1-            \ decrement inner
    repeat
    drop
  loop
;
\ i=0: inner=1, adds 1, acc=1
\ i=1: inner=2, adds 2, acc=3
\ i=2: inner=3, adds 3, acc=6
