\ Adversarial arithmetic test: MIN_INT64 * -1 (overflow case)
\ expect: 0
\ MIN_INT64 * -1 = MIN_INT64 (overflow! -MIN can't be represented)
\ On ARM64, MUL of MIN_INT64 * -1 produces MIN_INT64 (unchanged)
: main
  1 63 lshift       \ MIN_INT64
  -1 *              \ MIN_INT64 * -1
  1 63 lshift       \ MIN_INT64 again
  = if 0 else 1 then ;  \ Should equal MIN_INT64
