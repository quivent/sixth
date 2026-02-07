\ expect: 0
\ adversarial-2over-stressed.fs - Test 2over after heavy stack manipulation
\ Tests: Register state after dup swap rot over - do registers get clobbered?
\ Edge case: Register allocation, scratch registers corrupted
\
\ 2over: ( x1 x2 x3 x4 -- x1 x2 x3 x4 x1 x2 )
\ Bug to catch: 2over uses X9, X10 - if previous ops leave junk, it might fail

: verify-2over ( -- n )
  \ After 2over on 1 2 3 4, stack should be: 1 2 3 4 1 2
  \ Use subtraction/abs pattern, return 0 if correct
  2 - abs              \ check TOS = 2
  swap 1 - abs +       \ check next = 1
  swap 4 - abs +       \ check x4 = 4
  swap 3 - abs +       \ check x3 = 3
  swap 2 - abs +       \ check x2 = 2
  swap 1 - abs +       \ check x1 = 1
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
  \ Returns 0 if all matched, non-zero otherwise
;
