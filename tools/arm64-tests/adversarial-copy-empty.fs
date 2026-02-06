\ expect: 1
\ ADVERSARIAL: Empty string (length 0)
\ Tests CBZ X12, done - should skip the loop entirely
\ The loop should never execute when len=0

: main
  s" " 0 open-file    \ empty string open
  drop                \ drop ior
  \ fd could be negative (file not found is OK)
  \ but we should NOT crash - that's the test
  drop                \ drop fd
  1                   \ success = didn't crash
;
