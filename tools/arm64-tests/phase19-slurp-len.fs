\ expect: 1
\ Test slurp-file: verify length is reasonable (> 10 bytes)

: main
  s" tools/arm64-tests/phase1-exit42.fs" slurp-file  ( addr u ior )
  0= if                  \ ior = 0 means success
    10 > if 1 else 0 then  \ length should be > 10
  else
    drop 0               \ error: return 0
  then
;
