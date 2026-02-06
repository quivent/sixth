\ Adversarial arithmetic test: Division by zero behavior on ARM64
\ expect: 0
\ ARM64 SDIV by zero returns 0 (unlike x86 which traps)
: main
  42 0 / ;  \ Should return 0 on ARM64
