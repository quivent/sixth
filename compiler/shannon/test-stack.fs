\ Test stack.fs

\ Dependencies
create code-buf 256 allot
variable code-pos  0 code-pos !

: c, code-buf code-pos @ + c!  1 code-pos +! ;
: d, dup c, 8 rshift dup c, 8 rshift dup c, 8 rshift c, ;
: q, dup d, 32 rshift d, ;

: reset-code 0 code-pos ! ;

include compiler/shannon/stack.fs

\ Test infrastructure
variable tests 0 tests !
variable fails 0 fails !
: pass 1 tests +! ;
: fail 1 tests +! 1 fails +! ;

\ Tests
: test-registers
  ." Registers: "
  tos RAX = nos RBX = and third RCX = and stkptr R15 = and
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-depth-tracking
  ." Depth tracking: "
  0 stack-depth!
  stack-depth@ 0 =
  1 stack-depth+!
  stack-depth@ 1 = and
  2 stack-depth+!
  stack-depth@ 3 = and
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-push-emits-code
  ." push-val emits code: "
  reset-code
  0 stack-depth!
  push-val
  code-pos @ 0 >    \ Should have emitted mov rbx, rax
  stack-depth@ 1 = and
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-pop-emits-code
  ." pop-val emits code: "
  reset-code
  2 stack-depth!
  pop-val
  code-pos @ 0 >    \ Should have emitted mov rax, rbx
  stack-depth@ 1 = and
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-emit-lit
  ." emit-lit: "
  reset-code
  0 stack-depth!
  42 emit-lit
  code-pos @ 0 >
  stack-depth@ 1 = and
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-emit-lit-zero
  ." emit-lit 0 (xor): "
  reset-code
  0 stack-depth!
  0 emit-lit
  code-pos @ 6 =    \ push-val (3 bytes mov rbx,rax) + xor rax,rax (3 bytes with REX.W)
  if ." PASS" pass else ." FAIL" fail then cr ;

: run-tests
  cr ." === stack.fs Tests ===" cr cr
  test-registers
  test-depth-tracking
  test-push-emits-code
  test-pop-emits-code
  test-emit-lit
  test-emit-lit-zero
  cr ." Total: " tests @ . ." tests, " fails @ . ." failures" cr
  fails @ 0= if ." ALL PASS" else ." SOME FAILED" then cr ;

run-tests
bye
