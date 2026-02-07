\ expect: 42
\ Edge case test: MOD with negative dividends and divisors
\
\ Standard Forth semantics for SM/REM (symmetric division):
\   The sign of the remainder matches the sign of the dividend.
\   -17 mod 5 = -2 (because -17 = -3 * 5 + (-2))
\   17 mod -5 = 2 (because 17 = -3 * -5 + 2)
\   -17 mod -5 = -2
\
\ ARM64 SDIV + MSUB implements symmetric division (truncate toward zero)
\ So: remainder = dividend - (dividend / divisor) * divisor
\
\ Tests:
\ - Positive mod positive: 17 mod 5 = 2
\ - Negative mod positive: -17 mod 5 = -2
\ - Positive mod negative: 17 mod -5 = 2
\ - Negative mod negative: -17 mod -5 = -2
\
\ Expected:
\   17 mod 5 = 2
\   -17 mod 5 = -2, negate = 2
\   17 mod -5 = 2
\   -17 mod -5 = -2, negate = 2
\   2 + 2 + 2 + 2 = 8
\   We need 42, so: 8 + 34 = 42

: main
  \ Test 1: 17 mod 5 = 2
  17 5 mod          \ 2

  \ Test 2: -17 mod 5 = -2, negate to get 2
  -17 5 mod negate  \ 2
  +                 \ 4

  \ Test 3: 17 mod -5 = 2
  17 -5 mod         \ 2
  +                 \ 6

  \ Test 4: -17 mod -5 = -2, negate to get 2
  -17 -5 mod negate \ 2
  +                 \ 8

  \ Add 34 to get 42
  34 +              \ 42
;
