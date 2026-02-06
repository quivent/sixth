\ Adversarial test: LSHIFT by large values and roundtrip
\ 1 << 31 >> 31 should equal 1 (test large shift doesn't lose bits improperly)
\ expect: 1
: main
  1 31 lshift    \ 1 << 31 = 0x80000000 (bit 31 set)
  31 rshift      \ 0x80000000 >> 31 = 1
;
