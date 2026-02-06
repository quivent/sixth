\ adversarial-loop-05-leave-if-else.fs - LEAVE in both branches of IF
\ Loop 0-9, leave when i > 2
\ Iterations: 0, 1, 2, 3 (leave at i=3)
\ expect: 4

: main
  0  \ count
  10 0 do
    1+
    i 2 > if
      leave
    else
      \ continue
    then
  loop
;
