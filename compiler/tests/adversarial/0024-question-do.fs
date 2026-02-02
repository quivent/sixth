\ Adversarial test: ?do (zero-trip DO loop)
\ ?do skips the loop body entirely when start equals limit
\ Unlike regular do which always executes at least once

\ Test 1: ?do with start < limit (normal execution)
: test-qdo-normal ( -- )
  0
  5 0 ?do
    1+
  loop
  5 = if ." PASS: ?do normal loop executed 5 times" cr
  else ." FAIL: ?do normal loop iteration count wrong" cr then
;
test-qdo-normal

\ Test 2: ?do with start = limit (should skip entirely)
: test-qdo-zero-trip ( -- )
  999
  5 5 ?do
    drop 0
  loop
  999 = if ." PASS: ?do skipped when start=limit" cr
  else ." FAIL: ?do should skip when start=limit" cr then
;
test-qdo-zero-trip

\ Test 3: Another zero-trip test with different values
: test-qdo-zero-at-zero ( -- )
  42
  0 0 ?do
    drop -1
  loop
  42 = if ." PASS: ?do 0 0 skips correctly" cr
  else ." FAIL: ?do 0 0 should skip" cr then
;
test-qdo-zero-at-zero

\ Test 4: ?do with negative range (start < limit, negative numbers)
: test-qdo-negative ( -- )
  0
  0 -3 ?do
    1+
  loop
  3 = if ." PASS: ?do negative range executed 3 times" cr
  else ." FAIL: ?do negative range iteration wrong" cr then
;
test-qdo-negative

\ Test 5: ?do accessing loop index i
: test-qdo-index-sum ( -- )
  0
  4 0 ?do
    i +
  loop
  6 = if ." PASS: ?do index sum 0+1+2+3=6" cr
  else ." FAIL: ?do index sum wrong" cr then
;
test-qdo-index-sum

\ Test 6: ?do with start > limit (usually infinite or wraps, be careful)
\ This tests what happens - implementation dependent
: test-qdo-reverse ( -- n )
  0
  0 5 ?do
    1+
  -1 +loop
  \ Should count down: 5,4,3,2,1 = 5 iterations
;
test-qdo-reverse 5 = if ." PASS: ?do reverse loop 5 iterations" cr
else ." FAIL: ?do reverse loop count wrong" cr then

\ Test 7: Compare do vs ?do when start = limit
\ do always executes at least once, ?do skips
variable do-ran
variable qdo-ran

: test-do-executes ( -- )
  0 do-ran !
  3 3 do
    1 do-ran !
    leave
  loop
;

: test-qdo-skips ( -- )
  0 qdo-ran !
  3 3 ?do
    1 qdo-ran !
  loop
;

test-do-executes
test-qdo-skips

do-ran @ 1 = qdo-ran @ 0 = and
if ." PASS: do executes once, ?do skips when start=limit" cr
else ." FAIL: do/qdo behavior mismatch" cr then

\ Test 8: Nested ?do loops with zero-trip outer
: test-nested-qdo-outer-skip ( -- )
  0
  5 5 ?do
    10 0 ?do
      1+
    loop
  loop
  0 = if ." PASS: nested ?do outer skip works" cr
  else ." FAIL: nested ?do outer should skip" cr then
;
test-nested-qdo-outer-skip

\ Test 9: Nested ?do loops with zero-trip inner
: test-nested-qdo-inner-skip ( -- )
  0
  3 0 ?do
    1+
    5 5 ?do
      drop 999
    loop
  loop
  3 = if ." PASS: nested ?do inner skip works" cr
  else ." FAIL: nested ?do inner should skip each iteration" cr then
;
test-nested-qdo-inner-skip

\ Test 10: ?do with single iteration
: test-qdo-single ( -- )
  0
  1 0 ?do
    1+
  loop
  1 = if ." PASS: ?do single iteration works" cr
  else ." FAIL: ?do single iteration wrong" cr then
;
test-qdo-single

\ Test 11: ?do with leave
: test-qdo-leave ( -- )
  0
  10 0 ?do
    1+
    i 3 = if leave then
  loop
  4 = if ." PASS: ?do with leave exits at i=3" cr
  else ." FAIL: ?do leave behavior wrong" cr then
;
test-qdo-leave

\ Test 12: ?do zero-trip preserves stack
: test-qdo-stack-preserve ( -- )
  1 2 3
  0 0 ?do
    drop drop drop
    99 99 99 99
  loop
  3 = swap 2 = and swap 1 = and
  if ." PASS: ?do zero-trip preserves stack" cr
  else ." FAIL: ?do zero-trip corrupted stack" cr then
;
test-qdo-stack-preserve

\ REMINDER: ?do skips when start=limit. do does not. Use ?do when zero iterations is valid.
