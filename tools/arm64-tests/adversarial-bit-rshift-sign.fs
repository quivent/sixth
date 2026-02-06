\ Adversarial test: RSHIFT (logical) with high bit set
\ Logical right shift should NOT preserve sign bit
\ -1 (all ones) >> 63 should give 1, not -1
\ expect: 1
: main
  -1 63 rshift   \ logical shift: should be 1, not -1
;
