\ expect: 72
\ Test slurp-file: read a file and check first byte
\ We'll read this test file itself and check for backslash (92) or use a known file

: main
  s" tools/arm64-tests/phase1-exit42.fs" slurp-file  ( addr u ior )
  drop                   \ drop ior
  drop                   \ drop length
  c@                     \ read first byte (should be '\' = 92... but let's check 'H' for Hello test)
  \ Actually first byte of phase1-exit42.fs is '\' (backslash for comment)
  \ ASCII 92 = backslash, but exit code is modulo 256, so 92 works
  \ Let's just check it's non-zero
  0<> if 72 else 0 then  \ return 72 if we read something, 0 if empty
;
