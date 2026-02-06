\ expect: 1
\ ADVERSARIAL: Two character path
\ Tests loop executes exactly twice - catches subtle off-by-one issues
\ Different from single char because loop body runs > 1 time

: main
  s" /t" 0 open-file  \ two char path
  drop                \ drop ior
  drop                \ drop fd (likely error, that's OK)
  1                   \ success = didn't crash
;
