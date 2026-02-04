\ Minimal asm.fs test - no require, direct include

\ ============================================================
\ CODE EMISSION PRIMITIVES
\ ============================================================

create code-buf 256 allot
variable code-pos  0 code-pos !

: c, ( b -- ) code-buf code-pos @ + c!  1 code-pos +! ;
: d, ( d -- ) dup c, 8 rshift dup c, 8 rshift dup c, 8 rshift c, ;
: q, ( q -- ) dup d, 32 rshift d, ;

\ Load the assembler
include compiler/shannon/asm.fs

\ ============================================================
\ TEST INFRASTRUCTURE
\ ============================================================

: reset-code ( -- ) 0 code-pos ! ;
: byte@ ( offset -- byte ) code-buf + c@ ;

variable tests  0 tests !
variable fails  0 fails !

: pass  1 tests +! ;
: fail  1 tests +!  1 fails +! ;

: expect1 ( expected offset name-addr name-len -- )
  2>r byte@ = if pass else fail 2r> type ."  FAIL" cr exit then 2r> 2drop ;

: expect-bytes ( b0 b1 b2 len label-addr label-len -- )
  2>r
  code-pos @ <> if fail 2r> type ."  FAIL (len)" cr exit then
  2 byte@ <> if fail 2r> type ."  FAIL (b2)" cr exit then
  1 byte@ <> if fail 2r> type ."  FAIL (b1)" cr exit then
  0 byte@ <> if fail 2r> type ."  FAIL (b0)" cr exit then
  pass 2r> 2drop ;

\ ============================================================
\ TESTS
\ ============================================================

: test-registers
  ." Registers: "
  RAX 0 = RCX 1 = and RDX 2 = and RBX 3 = and
  RSP 4 = and RBP 5 = and RSI 6 = and RDI 7 = and
  R8 8 = and R9 9 = and R15 15 = and
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-mov-rr
  ." mov-rr RAX->RBX: "
  reset-code RAX RBX mov-rr
  code-pos @ 3 =
  0 byte@ $48 = and
  1 byte@ $89 = and
  2 byte@ $c3 = and
  if ." PASS" pass else ." FAIL" fail then cr

  ." mov-rr RBX->RAX: "
  reset-code RBX RAX mov-rr
  code-pos @ 3 =
  0 byte@ $48 = and
  1 byte@ $89 = and
  2 byte@ $d8 = and
  if ." PASS" pass else ." FAIL" fail then cr

  ." mov-rr R8->RAX: "
  reset-code R8 RAX mov-rr
  code-pos @ 3 =
  0 byte@ $4c = and
  1 byte@ $89 = and
  2 byte@ $c0 = and
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-add-rr
  ." add-rr RAX+RBX: "
  reset-code RAX RBX add-rr
  code-pos @ 3 =
  0 byte@ $48 = and
  1 byte@ $01 = and
  2 byte@ $c3 = and
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-inc-dec
  ." inc RAX: "
  reset-code RAX inc-r
  code-pos @ 3 =
  0 byte@ $48 = and
  1 byte@ $ff = and
  2 byte@ $c0 = and
  if ." PASS" pass else ." FAIL" fail then cr

  ." dec RAX: "
  reset-code RAX dec-r
  code-pos @ 3 =
  0 byte@ $48 = and
  1 byte@ $ff = and
  2 byte@ $c8 = and
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-push-pop
  ." push RAX: "
  reset-code RAX push-r
  code-pos @ 1 =
  0 byte@ $50 = and
  if ." PASS" pass else ." FAIL" fail then cr

  ." push RBX: "
  reset-code RBX push-r
  code-pos @ 1 =
  0 byte@ $53 = and
  if ." PASS" pass else ." FAIL" fail then cr

  ." pop RAX: "
  reset-code RAX pop-r
  code-pos @ 1 =
  0 byte@ $58 = and
  if ." PASS" pass else ." FAIL" fail then cr ;

: test-syscall-ret
  ." syscall: "
  reset-code syscall
  code-pos @ 2 =
  0 byte@ $0f = and
  1 byte@ $05 = and
  if ." PASS" pass else ." FAIL" fail then cr

  ." ret: "
  reset-code ret
  code-pos @ 1 =
  0 byte@ $c3 = and
  if ." PASS" pass else ." FAIL" fail then cr ;

: run-tests
  cr ." === asm.fs Tests ===" cr cr
  test-registers
  test-mov-rr
  test-add-rr
  test-inc-dec
  test-push-pop
  test-syscall-ret
  cr ." Total: " tests @ . ." tests, " fails @ . ." failures" cr
  fails @ 0= if ." ALL PASS" else ." SOME FAILED" then cr ;

run-tests
bye
