\ expect: 100 95 90 85 80
\ Deep pick stress test - goes 10+ elements deep
\ This breaks implementations that use limited registers or have stack pointer math bugs
: main
  100 99 98 97 96 95 94 93 92 91 90 89 88 87 86 85 84 83 82 81 80
  \ Stack: 100 at bottom, 80 at top (21 items)
  20 pick . cr   \ should be 100 (deepest)
  15 pick . cr   \ should be 95
  10 pick . cr   \ should be 90
  5 pick . cr    \ should be 85
  0 pick . cr    \ should be 80 (top)
  \ Clean up - drop everything
  drop drop drop drop drop drop drop drop drop drop
  drop drop drop drop drop drop drop drop drop drop
  drop drop drop drop drop drop
;
