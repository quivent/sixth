\ expect: 0
\ Adversarial: CRITICAL - tests signed vs unsigned comparison
\ -1 in unsigned is 0xFFFFFFFFFFFFFFFF (largest possible value)
\ -1 in signed is -1 (smaller than 1)
\ If using unsigned compare, min(-1,1) returns 1 (WRONG!)
\ Returns 0 if all pass (signed comparison correct)
: main
  -1 1 min -1 - abs      \ signed min(-1,1) = -1
  -1 1 max 1 - abs +     \ signed max(-1,1) = 1
  1 -1 min -1 - abs +    \ order reversed, still min = -1
  1 -1 max 1 - abs +     \ order reversed, still max = 1
;
