\ Adversarial arithmetic test: MIN_INT64 / -1 (overflow case)
\ expect: 0
\ MIN_INT64 / -1 overflows because -MIN_INT64 > MAX_INT64
\ ARM64: typically returns MIN_INT64 or 0 depending on implementation
: main
  1 63 lshift       \ MIN_INT64
  -1 /              \ MIN_INT64 / -1
  dup 1 63 lshift = if drop 0 else  \ Returns MIN_INT64 = PASS
  0 = if 0 else 1 then then ;       \ Returns 0 = also acceptable
