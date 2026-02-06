\ Adversarial: Mixed comparison chain with negations
\ expect: 1
: main
  \ Test multiple values in sequence
  100 0>=          \ true
  -100 0<=         \ true
  and
  -100 0>=         \ false
  or               \ true OR false = true
  100 0<=          \ false
  or               \ true OR false = true
  -1 = if 1 else 0 then ;
