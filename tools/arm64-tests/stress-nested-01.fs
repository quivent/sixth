\ Stress test: Triple nested DO-LOOP with I, J access
\ expect: 27
\ Count iterations: 3 * 3 * 3 = 27
: main
  0                     \ accumulator
  3 0 do                \ outer: I = 0, 1, 2
    3 0 do              \ middle: J = 0, 1, 2  (I still accessible)
      3 0 do            \ inner: (J visible as J, outer I visible as nothing standard)
        1 +             \ just count iterations
      loop
    loop
  loop ;                \ expect 27
