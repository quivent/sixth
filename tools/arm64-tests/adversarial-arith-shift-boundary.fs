\ expect: 42
\ Edge case test: Shift operations at boundaries (0, 63, 64 bits)
\
\ ARM64 shift semantics: shift amount is masked to 6 bits (0-63)
\ Shifting by 64 is equivalent to shifting by 0 on ARM64!
\ Shifting by 0 should return the value unchanged.
\ Shifting 1 left by 63 gives 0x8000000000000000 (MIN_INT64 signed)
\
\ Tests:
\ - lshift by 0 (identity)
\ - rshift by 0 (identity)
\ - lshift by 63 then rshift by 63 (should give 1 if implemented correctly)
\ - lshift by large value (behavior depends on masking)
\
\ Expected calculation:
\   10 lshift 0 = 10 (identity)
\   20 rshift 0 = 20 (identity)
\   1 lshift 3 = 8
\   8 rshift 3 = 1
\   10 + 20 + 8 + 1 = 39, need 3 more = 42

: shift-zero-test ( -- n )
  \ 10 lshift 0 should be 10
  10 0 lshift
  \ 20 rshift 0 should be 20
  20 0 rshift
  + ;  \ = 30

: shift-roundtrip ( n shift -- n result )
  \ shift left then right should recover original (if no overflow)
  \ NOTE: 2dup lshift swap rshift leaves BOTH n and the result
  2dup lshift swap rshift ;

: main
  shift-zero-test      \ 30
  \ 1 << 3 >> 3 = 1, but shift-roundtrip leaves (1 1)
  1 3 shift-roundtrip  \ Stack: 30 1 1
  drop                 \ Stack: 30 1 (drop extra n)
  +                    \ 31
  \ Add 8 (1 << 3) and 3
  1 3 lshift +         \ 31 + 8 = 39
  3 +                  \ 39 + 3 = 42
;
