\ adversarial-loop-03-cross-zero.fs - +LOOP crossing zero
\ From -5 to 5 by 2: -5, -3, -1, 1, 3 = 5 iterations
\ Sum: -5 + -3 + -1 + 1 + 3 = -5 (wraps to 251 as unsigned byte)
\ expect: 5

: main
  0  \ count iterations
  5 -5 do     \ from -5 to 5
    1+
    2 +loop   \ step by 2
  \ Exit code = iteration count = 5
;
