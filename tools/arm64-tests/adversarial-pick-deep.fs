\ expect: 0
\ Deep pick stress test - goes 10+ elements deep
\ This breaks implementations that use limited registers or have stack pointer math bugs
: main
  100 99 98 97 96 95 94 93 92 91 90 89 88 87 86 85 84 83 82 81 80
  \ Stack: 100 at bottom, 80 at top (21 items, indices 0-20)
  \ 0 pick = 80, 5 pick = 85, 10 pick = 90, 15 pick = 95, 20 pick = 100

  0   \ accumulator
  \ Stack now: 100..80 acc (22 items, acc at index 0)
  \ So original 0 pick (80) is now at index 1
  \ original 5 pick (85) is now at index 6, etc.

  1 pick 80 - abs +     \ was 0 pick, should be 80
  6 pick 85 - abs +     \ was 5 pick, should be 85
  11 pick 90 - abs +    \ was 10 pick, should be 90
  16 pick 95 - abs +    \ was 15 pick, should be 95
  21 pick 100 - abs +   \ was 20 pick, should be 100

  \ Clean up - drop the 21 items, leave accumulator
  >r
  drop drop drop drop drop drop drop drop drop drop
  drop drop drop drop drop drop drop drop drop drop
  drop
  r>
;
