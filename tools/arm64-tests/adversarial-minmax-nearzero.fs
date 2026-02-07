\ expect: 0
\ Adversarial: min/max near zero boundary
\ Tests -1, 0, 1 combinations - common off-by-one bugs
\ Returns 0 if all tests pass
: main
  -1 0 min -1 - abs      \ min(-1, 0) = -1
  -1 0 max abs +         \ max(-1, 0) = 0
  -1 1 min -1 - abs +    \ min(-1, 1) = -1 (signed!)
  -1 1 max 1 - abs +     \ max(-1, 1) = 1
  0 1 min abs +          \ min(0, 1) = 0
  0 1 max 1 - abs +      \ max(0, 1) = 1
;
