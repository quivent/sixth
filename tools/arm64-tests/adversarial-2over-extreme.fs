\ adversarial-2over-extreme.fs - Test 2over with extreme 64-bit values
\ Tests: MIN_INT64, MAX_INT64, 0, -1, 0x8000000000000000
\ Edge case: Sign extension, register width, extreme values surviving stack ops
\ expect: 1
\
\ 2over: ( x1 x2 x3 x4 -- x1 x2 x3 x4 x1 x2 )
\ If the implementation has sign or width bugs, these will expose them

\ MAX_INT64 = 0x7FFFFFFFFFFFFFFF = 9223372036854775807
\ MIN_INT64 = 0x8000000000000000 = -9223372036854775808 (signed)
\ Pattern uses 1 lshift repeatedly

: max-int64 ( -- n ) 1 62 lshift 1- 1 62 lshift or ;
: min-int64 ( -- n ) 1 63 lshift ;
: all-ones  ( -- n ) 0 invert ;

: check-pair ( got1 got2 exp1 exp2 -- flag )
  \ Check if got1=exp1 AND got2=exp2
  rot = -rot = and ;

: main
  \ Test 1: 2over with MIN_INT64 and MAX_INT64
  min-int64 max-int64    \ x1 x2 (second pair - to be copied)
  0 all-ones              \ x3 x4 (top pair)
  2over                   \ should produce: min max 0 -1 min max

  \ Stack: min max 0 -1 min max (TOS=max)
  \ Verify: TOS should be max-int64, NOS should be min-int64
  max-int64 = swap       \ ( min max 0 -1 min flag )
  min-int64 = and        \ ( min max 0 -1 flag )

  \ Now verify x3 x4 unchanged
  swap all-ones = and    \ check x4=-1
  swap 0= and            \ check x3=0

  \ Verify original x1 x2 still there
  swap max-int64 = and   \ check x2=max
  swap min-int64 = and   \ check x1=min

  \ Convert to 1 or 0
  if 1 else 0 then
;
