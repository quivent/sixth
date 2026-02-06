\ Adversarial test: LSHIFT boundary cases
\ Shift by 0 = no change
\ Shift by 63 = only MSB position (on 64-bit)
\ expect: 1
: main
  1 0 lshift   \ 1 << 0 = 1
;
