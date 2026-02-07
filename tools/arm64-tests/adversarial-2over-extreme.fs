\ expect: 0
\ adversarial-2over-extreme.fs - Test 2over with extreme 64-bit values
\ Tests: MIN_INT64, MAX_INT64, 0, -1, 0x8000000000000000
\ Edge case: Sign extension, register width, extreme values surviving stack ops
\
\ 2over: ( x1 x2 x3 x4 -- x1 x2 x3 x4 x1 x2 )
\ If the implementation has sign or width bugs, these will expose them

\ MAX_INT64 = 0x7FFFFFFFFFFFFFFF = 9223372036854775807
\ MIN_INT64 = 0x8000000000000000 = -9223372036854775808 (signed)
\ Pattern uses 1 lshift repeatedly

: max-int64 ( -- n ) 1 62 lshift 1- 1 62 lshift or ;
: min-int64 ( -- n ) 1 63 lshift ;
: all-ones  ( -- n ) 0 invert ;

: main
  \ Test: 2over with MIN_INT64 and MAX_INT64
  min-int64 max-int64    \ x1 x2 (second pair - to be copied)
  0 all-ones             \ x3 x4 (top pair)
  2over                  \ should produce: min max 0 -1 min max

  \ Stack: min max 0 -1 min max (TOS=max)
  \ Verify using xor (0 if equal) and accumulate errors
  max-int64 xor abs      \ check TOS = max-int64
  swap min-int64 xor abs +  \ check next = min-int64
  swap all-ones xor abs +   \ check x4 = -1
  swap 0 xor abs +          \ check x3 = 0
  swap max-int64 xor abs +  \ check x2 = max-int64
  swap min-int64 xor abs +  \ check x1 = min-int64
  \ Returns 0 if all matched, non-zero otherwise
;
