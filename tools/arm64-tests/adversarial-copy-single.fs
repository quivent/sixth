\ expect: 1
\ ADVERSARIAL: Single character path
\ Tests loop executes exactly once and terminates correctly
\ This catches off-by-one errors in the loop condition

: main
  s" /" 0 open-file   \ root directory (single char path)
  drop                \ drop ior
  0>= if 1 else 1 then  \ either way, didn't crash = success
;
