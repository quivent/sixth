\ Adversarial test: Bit extraction pattern
\ Extract bits 4-7 from value 0xABCD = shift right 4, AND with 0xF
\ 0xABCD = 43981, >> 4 = 2748, AND 0xF = 12 (0xC)
\ expect: 12
: main
  43981 4 rshift 15 and   \ extract nibble
;
