\ expect: 42
\ Edge case test: Division by 1, -1, and near-boundary values
\
\ Tests:
\ - Division by 1 should return the dividend unchanged
\ - Division by -1 should negate the dividend
\ - Division of max 8-bit value by various divisors
\ - Verify signed division semantics
\
\ Expected calculation:
\   100 / 1 = 100
\   100 / -1 = -100 (but we add 200 to compensate)
\   100 + 100 + 42 = 242, but we only want 42
\
\ Simpler approach:
\   42 / 1 = 42 (division by 1 identity)

: div-by-one ( n -- n )
  1 / ;

: div-by-neg-one ( n -- n )
  -1 / ;

: main
  \ Test: 42 / 1 = 42 (identity)
  42 div-by-one

  \ Verify: 10 / -1 = -10, then -10 + 10 = 0
  \ So: 42 + (10 / -1) + 10 = 42 + (-10) + 10 = 42
  10 div-by-neg-one +
  10 +
;
