\ Test optimization modules

\ Dependencies
create code-buf 256 allot
variable code-pos  0 code-pos !

: c, code-buf code-pos @ + c!  1 code-pos +! ;
: d, dup c, 8 rshift dup c, 8 rshift dup c, 8 rshift c, ;
: q, dup d, 32 rshift d, ;

include compiler/shannon/asm.fs
include compiler/shannon/stack.fs
include compiler/shannon/prims.fs
include compiler/shannon/opt-fold.fs
include compiler/shannon/opt-fuse.fs
include compiler/shannon/opt-swap.fs

: reset-code 0 code-pos ! 0 stack-depth ! ;

\ Test infrastructure
variable tests 0 tests !
variable fails 0 fails !
: pass 1 tests +! ;
: fail 1 tests +! 1 fails +! ;

\ ============================================================
\ opt-fold tests
\ ============================================================

: test-ct-push-pop
  ." ct-push/pop: "
  ct-reset
  42 ct-push
  ct-depth@ 1 =
  ct-pop 42 = and
  ct-depth@ 0 = and
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-fold-add
  ." fold-add: "
  ct-reset
  3 ct-push 4 ct-push
  fold-add
  ct-depth@ 1 =
  ct-pop 7 = and
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-fold-mul
  ." fold-mul: "
  ct-reset
  5 ct-push 6 ct-push
  fold-mul
  ct-depth@ 1 =
  ct-pop 30 = and
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-fold-sub
  ." fold-sub (10-3=7): "
  ct-reset
  10 ct-push 3 ct-push
  fold-sub
  ct-pop 7 =
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-ct-flush
  ." ct-flush emits lit: "
  ct-reset
  reset-code
  42 ct-push
  ct-flush
  code-pos @ 0 >      \ should have emitted mov rax, 42
  ct-depth@ 0 = and
  stack-depth @ 1 = and
  if ." PASS" pass else ." FAIL" fail then cr ;

\ ============================================================
\ opt-fuse tests
\ ============================================================

: test-power-of-2
  ." power-of-2?: "
  1 power-of-2?
  2 power-of-2? and
  4 power-of-2? and
  8 power-of-2? and
  256 power-of-2? and
  3 power-of-2? 0= and
  5 power-of-2? 0= and
  0 power-of-2? 0= and
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-log2
  ." log2: "
  1 log2 0 =
  2 log2 1 = and
  4 log2 2 = and
  8 log2 3 = and
  256 log2 8 = and
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-fuse-add
  ." fuse-add: "
  ct-reset
  reset-code
  1 stack-depth !
  10 ct-push
  fuse-add
  code-pos @ 0 >      \ should emit add rax, 10
  ct-depth@ 0 = and
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-fuse-mul-shift
  ." fuse-mul (8->shl 3): "
  ct-reset
  reset-code
  1 stack-depth !
  8 ct-push
  fuse-mul
  code-pos @ 0 >
  ct-depth@ 0 = and
  if ." PASS" pass else ." FAIL" fail then cr ;

\ ============================================================
\ opt-swap tests
\ ============================================================

: test-swap-pending
  ." swap-pending: "
  clear-swap
  swap-pending? 0=
  mark-swap
  swap-pending? and
  clear-swap
  swap-pending? 0= and
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-cancel-swap
  ." cancel-swap: "
  clear-swap
  mark-swap
  swap-pending?
  cancel-swap         \ swap swap = nothing
  swap-pending? 0= and
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-flush-swap
  ." flush-swap emits xchg: "
  reset-code
  2 stack-depth !
  clear-swap
  mark-swap
  flush-swap
  code-pos @ 0 >      \ should emit xchg rax, rbx
  swap-pending? 0= and
  if ." PASS" pass else ." FAIL" fail then cr ;

\ ============================================================
\ Run all tests
\ ============================================================

: run-tests
  cr ." === Optimization Module Tests ===" cr
  cr ." --- opt-fold.fs ---" cr
  test-ct-push-pop
  test-fold-add
  test-fold-mul
  test-fold-sub
  test-ct-flush
  cr ." --- opt-fuse.fs ---" cr
  test-power-of-2
  test-log2
  test-fuse-add
  test-fuse-mul-shift
  cr ." --- opt-swap.fs ---" cr
  test-swap-pending
  test-cancel-swap
  test-flush-swap
  cr ." Total: " tests @ . ." tests, " fails @ . ." failures" cr
  fails @ 0= if ." ALL PASS" else ." SOME FAILED" then cr ;

run-tests
bye
