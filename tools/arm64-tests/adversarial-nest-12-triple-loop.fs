\ Adversarial test: Triple nested DO-LOOP
\ Tests three levels of loop nesting with index access
\ expect: 27

: main
  0           \ accumulator
  3 0 do      \ i = 0,1,2
    3 0 do    \ j = 0,1,2 for each i
      3 0 do  \ k = 0,1,2 for each (i,j)
        1+    \ count iterations
      loop
    loop
  loop
;
\ 3 * 3 * 3 = 27 iterations
