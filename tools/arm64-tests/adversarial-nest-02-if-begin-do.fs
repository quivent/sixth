\ Adversarial test: IF inside BEGIN..UNTIL inside DO..LOOP
\ Complex three-level nesting of different control structures
\ expect: 3

: main
  0                      \ accumulator
  3 0 do                 \ outer: 3 iterations (i=0,1,2)
    i 1+                 \ counter starts at i+1
    begin
      1+                 \ increment counter
      dup 5 >            \ until counter > 5
    until
    drop                 \ drop counter
    i +                  \ add outer index to acc
  loop
;
\ i=0: acc = 0 + 0 = 0
\ i=1: acc = 0 + 1 = 1
\ i=2: acc = 1 + 2 = 3
