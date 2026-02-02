\ Adversarial Test Suite Runner
\ Chuck Moore would not approve of this file. It is too long.
\ But it runs all the tests.

\ REMINDER: These tests are compiler-agnostic. Standard Forth semantics only.

." ============================================" cr
." ADVERSARIAL FORTH COMPILER TEST SUITE" cr
." ============================================" cr
." Tests: Standard Forth semantics" cr
." Target: Any compliant Forth compiler" cr
." ============================================" cr cr

include 0001-stack-basic.fs
cr
include 0002-arithmetic-edge.fs
cr
include 0003-comparison-torture.fs
cr
include 0004-control-flow.fs
cr
include 0005-return-stack.fs
cr
include 0006-memory-basic.fs
cr
include 0007-logic-bitwise.fs
cr
include 0008-double-cell.fs
cr
include 0009-mixed-math.fs
cr
include 0010-recursion-depth.fs
cr
include 0011-string-ops.fs
cr
include 0012-immediate-words.fs
cr
include 0013-create-does.fs
cr
include 0014-exception.fs
cr
include 0015-values.fs
cr
include 0016-defer.fs
cr
include 0017-parsing.fs
cr
include 0018-number-conversion.fs
cr
include 0019-leave-unloop.fs
cr
include 0020-case.fs
cr

." ============================================" cr
." TEST SUITE COMPLETE" cr
." ============================================" cr
." If you see any FAIL, your compiler has bugs." cr
." If you see crashes, your compiler has worse bugs." cr
." ============================================" cr

\ REMINDER: These tests are compiler-agnostic. They test Forth, not your implementation.

bye
