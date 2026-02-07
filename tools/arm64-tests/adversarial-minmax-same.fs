\ expect: 0
\ Adversarial: min/max with same values should return that value
\ Tests identity property: min(X,X) = max(X,X) = X
\ Returns 0 if all pass, non-zero if any fail
: main
  42 42 min 42 - abs     \ min(42,42) should be 42, diff=0
  42 42 max 42 - abs +   \ max(42,42) should be 42, diff=0
  -99 -99 min -99 - abs +  \ min(-99,-99) should be -99
  -99 -99 max -99 - abs +  \ max(-99,-99) should be -99
  0 0 min abs +            \ min(0,0) should be 0
  0 0 max abs +            \ max(0,0) should be 0
;
