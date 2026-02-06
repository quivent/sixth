\ Adversarial test: AND with zero
\ AND with 0 should always return 0 regardless of input
\ expect: 0
: main
  -1 0 and   \ all-ones AND 0 = 0
;
