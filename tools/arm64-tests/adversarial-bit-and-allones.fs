\ Adversarial test: AND with all-ones pattern
\ AND with -1 (all bits set) should return the original value
\ 0x5555555555555555 AND -1 = 0x5555555555555555
\ expect: 5
: main
  5 -1 and   \ 5 AND all-ones = 5
;
