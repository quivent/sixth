\ Adversarial test: INVERT all bits
\ INVERT 0 = -1 (all bits set)
\ INVERT of INVERT = original value
\ expect: 0
: main
  0 invert invert   \ double invert = original
;
