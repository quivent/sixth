\ adversarial-loop-01-nested4.fs - 4+ levels of nested DO-LOOP
\ Test: 3*2*2*2 = 24 iterations
\ expect: 24

: main
  0  \ accumulator
  3 0 do          \ level 1: 0,1,2
    2 0 do        \ level 2: 0,1
      2 0 do      \ level 3: 0,1
        2 0 do    \ level 4: 0,1
          1+
        loop
      loop
    loop
  loop
  \ Result is on TOS, becomes exit code
;
