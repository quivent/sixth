\ expect: 0
\ adversarial-2over-chain.fs - Test multiple 2over in sequence
\ Tests: Back-to-back 2over calls, stack growing correctly
\ Edge case: Each 2over adds 2 cells, offset calculations must track depth
\
\ 2over: ( x1 x2 x3 x4 -- x1 x2 x3 x4 x1 x2 )
\ Bug to catch: Static offsets that don't account for growing stack

: main
  \ Use distinct values that show exactly which positions are being copied
  100 200 300 400

  \ First 2over: 100 200 300 400 -> 100 200 300 400 100 200
  2over
  \ Stack: 100 200 300 400 100 200 (TOS=200)

  \ Second 2over on 6-item stack:
  \ Looking at top 4: 300 400 100 200
  \ "Second pair" from top means 300 400
  \ Result: 100 200 300 400 100 200 300 400
  2over
  \ Stack: 100 200 300 400 100 200 300 400 (TOS=400)

  \ Third 2over on 8-item stack:
  \ Looking at top 4: 100 200 300 400
  \ "Second pair" = 100 200
  \ Result: 100 200 300 400 100 200 300 400 100 200
  2over
  \ Stack: 100 200 300 400 100 200 300 400 100 200 (TOS=200)

  \ Now verify all 10 values from TOS down:
  \ TOS: 200 100 400 300 200 100 400 300 200 100
  200 - abs
  swap 100 - abs +
  swap 400 - abs +
  swap 300 - abs +
  swap 200 - abs +
  swap 100 - abs +
  swap 400 - abs +
  swap 300 - abs +
  swap 200 - abs +
  swap 100 - abs +
  \ Returns 0 if all matched, non-zero otherwise
;
