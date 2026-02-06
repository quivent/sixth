\ expect: 99
\ Test: Alternating dup/drop chaos - tests stack pointer tracking
: main
  99
  dup dup dup dup dup dup dup dup dup dup
  drop drop drop drop drop drop drop drop drop drop
  dup dup dup dup dup dup dup dup dup dup
  dup dup dup dup dup dup dup dup dup dup
  drop drop drop drop drop drop drop drop drop drop
  drop drop drop drop drop drop drop drop drop drop
  dup dup dup dup dup
  drop drop drop drop drop
;
