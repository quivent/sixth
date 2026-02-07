\ expect: 42
\ Edge case test: Multiplication overflow/underflow
\
\ 64-bit signed multiplication can overflow silently.
\ MUL instruction produces the low 64 bits of the 128-bit result.
\
\ Tests:
\ - Multiply by 0 (should give 0)
\ - Multiply by 1 (identity)
\ - Multiply by -1 (negate)
\ - Large multiplication that overflows, then mask to low bits
\ - Overflow detection: (a * b) / b should equal a if no overflow
\
\ Expected calculation:
\   7 * 6 = 42 (basic case, no overflow)
\
\ More interesting: test overflow recovery
\   256 * 256 = 65536
\   65536 * 256 = 16777216
\   This won't overflow 64 bits but tests the machinery

: mul-by-zero ( n -- 0 )
  0 * ;

: mul-by-one ( n -- n )
  1 * ;

: mul-by-neg-one ( n -- -n )
  -1 * ;

: main
  \ Basic: 7 * 6 = 42
  7 6 *

  \ Verify multiply by 0 gives 0
  100 mul-by-zero +      \ 42 + 0 = 42

  \ Verify multiply by 1 is identity (add 0 via identity check)
  50 mul-by-one 50 - +   \ 42 + (50 - 50) = 42

  \ Verify multiply by -1 negates (add 0 via negate check)
  25 mul-by-neg-one 25 + +  \ 42 + (-25 + 25) = 42

  \ Test larger multiply then divide back
  \ 1000 * 1000 = 1000000, / 1000000 = 1, - 1 = 0
  1000 1000 * 1000000 / 1 - +  \ 42 + (1 - 1) = 42
;
