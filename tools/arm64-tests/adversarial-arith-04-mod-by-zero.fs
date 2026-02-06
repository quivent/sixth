\ Adversarial arithmetic test: Modulo by zero behavior
\ expect: 42
\ ARM64: 42 / 0 = 0, so 42 mod 0 = 42 - 0*0 = 42
: main
  42 0 mod ;  \ Should return 42 (dividend unchanged when divisor is 0)
