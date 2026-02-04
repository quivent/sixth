\ Test dispatch.fs

create code-buf 256 allot
variable code-pos  0 code-pos !
: c, code-buf code-pos @ + c!  1 code-pos +! ;
: d, dup c, 8 rshift dup c, 8 rshift dup c, 8 rshift c, ;
: q, dup d, 32 rshift d, ;

include compiler/shannon/dispatch.fs

variable tests 0 tests !
variable fails 0 fails !
: pass 1 tests +! ;
: fail 1 tests +! 1 fails +! ;

: test-find-plus
  ." find '+': "
  s" +" find-builtin
  if
    F_FOLD2C and F_FOLD2C =
    if ." PASS" pass else ." FAIL (wrong flags)" fail then
  else ." FAIL (not found)" fail then cr ;

: test-find-dup
  ." find 'dup': "
  s" dup" find-builtin
  if
    F_STACKOP and 0 <>
    if ." PASS" pass else ." FAIL (wrong flags)" fail then
  else ." FAIL (not found)" fail then cr ;

: test-find-cr
  ." find 'cr': "
  s" cr" find-builtin
  if
    F_IO and 0 <>
    if ." PASS" pass else ." FAIL (wrong flags)" fail then
  else ." FAIL (not found)" fail then cr ;

: test-find-if
  ." find 'if': "
  s" if" find-builtin
  if
    F_CONTROL and 0 <>
    if ." PASS" pass else ." FAIL (wrong flags)" fail then
  else ." FAIL (not found)" fail then cr ;

: test-find-unknown
  ." find 'xyz123': "
  s" xyz123" find-builtin
  if ." FAIL (should not find)" fail
  else ." PASS" pass then cr ;

: test-commutative
  ." commutative? '+': "
  s" +" find-builtin if commutative? else false then
  if ." PASS" pass else ." FAIL" fail then cr ;

: run-tests
  cr ." === dispatch.fs Tests ===" cr cr
  test-find-plus
  test-find-dup
  test-find-cr
  test-find-if
  test-find-unknown
  test-commutative
  cr ." Total: " tests @ . ." tests, " fails @ . ." failures" cr
  fails @ 0= if ." ALL PASS" else ." SOME FAILED" then cr ;

run-tests
bye
