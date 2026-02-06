\ Adversarial test: Bitwise ops with negative numbers
\ -1 AND 255 = 255 (masking with 0xFF extracts low byte)
\ expect: 255
: main
  -1 255 and   \ mask to low byte
;
