\ Test prims.fs

\ Dependencies
create code-buf 256 allot
variable code-pos  0 code-pos !

: c, code-buf code-pos @ + c!  1 code-pos +! ;
: d, dup c, 8 rshift dup c, 8 rshift dup c, 8 rshift c, ;
: q, dup d, 32 rshift d, ;

include compiler/shannon/asm.fs
include compiler/shannon/stack.fs
include compiler/shannon/prims.fs

: reset-code 0 code-pos ! 0 stack-depth ! ;
: byte@ code-buf + c@ ;

\ Test infrastructure
variable tests 0 tests !
variable fails 0 fails !
: pass 1 tests +! ;
: fail 1 tests +! 1 fails +! ;

\ Tests
: test-emit-dup
  ." emit-dup: "
  reset-code
  1 stack-depth !
  emit-dup
  code-pos @ 0 >        \ emits mov rbx, rax
  stack-depth @ 2 = and
  if ." PASS" pass else ." FAIL (depth=" stack-depth @ . ." )" fail then cr ;

: test-emit-drop
  ." emit-drop: "
  reset-code
  2 stack-depth !
  emit-drop
  code-pos @ 0 >        \ emits mov rax, rbx
  stack-depth @ 1 = and
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-emit-swap
  ." emit-swap: "
  reset-code
  2 stack-depth !
  emit-swap
  code-pos @ 0 >        \ emits xchg rax, rbx
  stack-depth @ 2 = and \ depth unchanged
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-emit-add
  ." emit-add: "
  reset-code
  2 stack-depth !
  emit-add
  code-pos @ 0 >
  stack-depth @ 1 = and \ consumes 2, produces 1
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-emit-sub
  ." emit-sub: "
  reset-code
  2 stack-depth !
  emit-sub
  code-pos @ 0 >
  stack-depth @ 1 = and
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-emit-mul
  ." emit-mul: "
  reset-code
  2 stack-depth !
  emit-mul
  code-pos @ 0 >
  stack-depth @ 1 = and
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-emit-negate
  ." emit-negate: "
  reset-code
  1 stack-depth !
  emit-negate
  code-pos @ 0 >
  stack-depth @ 1 = and \ depth unchanged
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-emit-fetch
  ." emit-@: "
  reset-code
  1 stack-depth !
  emit-@
  code-pos @ 0 >
  stack-depth @ 1 = and \ depth unchanged (addr -> value)
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-emit-store
  ." emit-!: "
  reset-code
  2 stack-depth !
  emit-!
  code-pos @ 0 >
  stack-depth @ 0 = and \ consumes 2
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-emit-and
  ." emit-and: "
  reset-code
  2 stack-depth !
  emit-and
  code-pos @ 0 >
  stack-depth @ 1 = and
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-emit-add-imm
  ." emit-add-imm: "
  reset-code
  1 stack-depth !
  42 emit-add-imm
  code-pos @ 0 >
  stack-depth @ 1 = and
  if ." PASS" pass else ." FAIL" fail then cr ;

: run-tests
  cr ." === prims.fs Tests ===" cr cr
  test-emit-dup
  test-emit-drop
  test-emit-swap
  test-emit-add
  test-emit-sub
  test-emit-mul
  test-emit-negate
  test-emit-fetch
  test-emit-store
  test-emit-and
  test-emit-add-imm
  cr ." Total: " tests @ . ." tests, " fails @ . ." failures" cr
  fails @ 0= if ." ALL PASS" else ." SOME FAILED" then cr ;

run-tests
bye
