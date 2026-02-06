\ expect: 0
\ ADVERSARIAL: Test +! near maximum values (overflow wrapping)
\ Tests 64-bit arithmetic overflow behavior
\ -1 is max unsigned value, +1 should wrap to 0

variable bignum

: main
  -1 bignum !             \ store 0xFFFFFFFFFFFFFFFF (max unsigned)
  1 bignum +!             \ add 1 (should overflow to 0)
  bignum @                \ should be 0
;
