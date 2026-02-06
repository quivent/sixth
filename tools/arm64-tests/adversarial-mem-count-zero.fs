\ expect: 0
\ ADVERSARIAL: Count with zero-length counted string
\ Tests edge case where length byte is 0
\ Should return address+1 and length 0

: main
  here                    \ address for counted string
  0 over c!               \ store length 0
  count                   \ should give addr+1, len=0
  swap drop               \ drop addr, keep length
;
