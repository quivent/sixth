\ Adversarial test: XOR for bit toggling (self-inverse)
\ x XOR x = 0 (property used for swapping without temp)
\ Then XOR with another value toggles those bits
\ expect: 0
: main
  12345 12345 xor   \ any value XOR itself = 0
;
