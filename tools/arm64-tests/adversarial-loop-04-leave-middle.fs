\ adversarial-loop-04-leave-middle.fs - LEAVE from middle nesting level
\ Outer loop 0,1,2; inner loop leaves when i=j
\ j=0: leave at i=0 (1 iteration)
\ j=1: leave at i=1 (2 iterations)
\ j=2: leave at i=2 (3 iterations)
\ Total = 1+2+3 = 6
\ expect: 6

: main
  0  \ count
  3 0 do          \ outer: j=0,1,2
    3 0 do        \ inner: i=0,1,2
      1+          \ count each inner iteration
      i j = if leave then
    loop
  loop
;
