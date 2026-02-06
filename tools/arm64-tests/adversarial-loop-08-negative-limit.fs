\ adversarial-loop-08-negative-limit.fs - DO-LOOP with negative limit
\ From -10 to -5: 5 iterations
\ Sum: -10 + -9 + -8 + -7 + -6 = -40
\ Exit code will be -40 mod 256 = 216
\ expect: 5

: main
  0  \ count
  -5 -10 do   \ from -10 to -5 (exclusive)
    1+
  loop
;
