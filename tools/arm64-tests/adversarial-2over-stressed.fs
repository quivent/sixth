\ adversarial-2over-stressed.fs - Test 2over after heavy stack manipulation
\ Tests: Register state after dup swap rot over - do registers get clobbered?
\ Edge case: Register allocation, scratch registers corrupted
\ expect: 1
\
\ 2over: ( x1 x2 x3 x4 -- x1 x2 x3 x4 x1 x2 )
\ Bug to catch: 2over uses X9, X10 - if previous ops leave junk, it might fail

: stress-stack ( -- 1 2 3 4 )
  \ Heavy manipulation to pollute registers
  10 20 30 40 50 60 70 80
  swap drop swap drop swap drop swap drop
  \ Stack: 10 30 50 70
  dup over + swap -
  \ Stack: 10 30 50 (70+50-70=50)
  drop drop
  \ Stack: 10 30
  rot rot rot
  \ Oops, need 4 items. Rebuild.
  1 2 3 4
;

: verify-2over ( -- flag )
  \ After 2over on 1 2 3 4, stack should be: 1 2 3 4 1 2
  2 = swap 1 = and      \ check copied pair
  swap 4 = and          \ check x4
  swap 3 = and          \ check x3
  swap 2 = and          \ check x2
  swap 1 = and          \ check x1
;

: main
  \ First stress the stack
  10 20 30 40 dup drop swap swap rot -rot over nip tuck
  2drop 2drop 2drop

  \ Now the actual test
  1 2 3 4

  \ More stress right before 2over
  dup drop dup drop dup drop dup drop

  2over

  verify-2over
  if 1 else 0 then
;
