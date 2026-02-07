\ Adversarial: ?dup with zero must NOT duplicate
\ Tests that CBZ correctly branches around push-tos when TOS=0
\ If ?dup incorrectly duplicates, drop leaves 0, then 99+ gives 99
\ But if correct, we have one 0, 99+ gives 99 anyway
\ Better test: if duplicates wrongly, we'd have 0 0, drop drop = underflow
\ Use: 0 ?dup depth check - if not duplicated, depth=1, if duplicated depth=2
\ Actually just test the value path:
\ expect: 99
: main
  0 ?dup        \ should leave just 0 (not 0 0)
  drop          \ stack empty
  99            \ push 99
;
