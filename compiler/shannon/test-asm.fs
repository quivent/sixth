\ test-asm.fs - Test harness for asm.fs x86-64 assembler
\ Run: ./engine/fifth compiler/shannon/test-asm.fs

\ ============================================================
\ CODE EMISSION PRIMITIVES (must be defined before asm.fs)
\ ============================================================

create code-buf 256 allot
variable code-pos  0 code-pos !

: c, ( b -- ) code-buf code-pos @ + c!  1 code-pos +! ;
: d, ( d -- ) dup c, 8 rshift dup c, 8 rshift dup c, 8 rshift c, ;
: q, ( q -- ) dup d, 32 rshift d, ;

\ Now load the assembler
require compiler/shannon/asm.fs

\ ============================================================
\ TEST INFRASTRUCTURE
\ ============================================================

: reset-code ( -- ) 0 code-pos ! ;

variable pass-count  0 pass-count !
variable fail-count  0 fail-count !

: byte@ ( offset -- byte ) code-buf + c@ ;

: show-got ( -- )
  ."   Got:      "
  code-pos @ 0 do code-buf i + c@ . loop cr ;

: check-result ( flag -- )
  if
    pass-count @ 1+ pass-count !
    ." PASS" cr
  else
    fail-count @ 1+ fail-count !
    ." FAIL" cr show-got
  then ;

\ ============================================================
\ TESTS
\ ============================================================

: test-mov-rr
  ." [mov-rr]" cr

  ." RAX RBX mov-rr: "
  reset-code RAX RBX mov-rr
  code-pos @ 3 = 0 byte@ $48 = and 1 byte@ $89 = and 2 byte@ $c3 = and
  check-result

  ." RBX RAX mov-rr: "
  reset-code RBX RAX mov-rr
  code-pos @ 3 = 0 byte@ $48 = and 1 byte@ $89 = and 2 byte@ $d8 = and
  check-result

  ." R8 RAX mov-rr: "
  reset-code R8 RAX mov-rr
  code-pos @ 3 = 0 byte@ $4c = and 1 byte@ $89 = and 2 byte@ $c0 = and
  check-result

  ." RAX R8 mov-rr: "
  reset-code RAX R8 mov-rr
  code-pos @ 3 = 0 byte@ $49 = and 1 byte@ $89 = and 2 byte@ $c0 = and
  check-result

  ." R8 R9 mov-rr: "
  reset-code R8 R9 mov-rr
  code-pos @ 3 = 0 byte@ $4d = and 1 byte@ $89 = and 2 byte@ $c1 = and
  check-result
;

: test-mov-ri
  cr ." [mov-ri32]" cr

  ." $12345678 RAX mov-ri32: "
  reset-code $12345678 RAX mov-ri32
  code-pos @ 7 = 0 byte@ $48 = and 1 byte@ $c7 = and 2 byte@ $c0 = and
  3 byte@ $78 = and 4 byte@ $56 = and 5 byte@ $34 = and 6 byte@ $12 = and
  check-result

  cr ." [mov-ri (64-bit)]" cr

  ." 64-bit RAX mov-ri: "
  reset-code $123456789ABCDEF0 RAX mov-ri
  code-pos @ 10 = 0 byte@ $48 = and 1 byte@ $b8 = and
  2 byte@ $f0 = and 3 byte@ $de = and 4 byte@ $bc = and 5 byte@ $9a = and
  6 byte@ $78 = and 7 byte@ $56 = and 8 byte@ $34 = and 9 byte@ $12 = and
  check-result

  ." 64-bit R8 mov-ri: "
  reset-code $0807060504030201 R8 mov-ri
  code-pos @ 10 = 0 byte@ $49 = and 1 byte@ $b8 = and
  2 byte@ $01 = and 3 byte@ $02 = and 4 byte@ $03 = and 5 byte@ $04 = and
  6 byte@ $05 = and 7 byte@ $06 = and 8 byte@ $07 = and 9 byte@ $08 = and
  check-result
;

: test-add-sub
  cr ." [add-rr]" cr

  ." RAX RBX add-rr: "
  reset-code RAX RBX add-rr
  code-pos @ 3 = 0 byte@ $48 = and 1 byte@ $01 = and 2 byte@ $c3 = and
  check-result

  cr ." [add-ri]" cr

  ." $1000 RAX add-ri: "
  reset-code $1000 RAX add-ri
  code-pos @ 6 = 0 byte@ $48 = and 1 byte@ $05 = and
  2 byte@ $00 = and 3 byte@ $10 = and 4 byte@ $00 = and 5 byte@ $00 = and
  check-result

  ." $1000 RBX add-ri: "
  reset-code $1000 RBX add-ri
  code-pos @ 7 = 0 byte@ $48 = and 1 byte@ $81 = and 2 byte@ $c3 = and
  3 byte@ $00 = and 4 byte@ $10 = and 5 byte@ $00 = and 6 byte@ $00 = and
  check-result

  cr ." [sub-rr]" cr

  ." RAX RBX sub-rr: "
  reset-code RAX RBX sub-rr
  code-pos @ 3 = 0 byte@ $48 = and 1 byte@ $29 = and 2 byte@ $c3 = and
  check-result
;

: test-unary
  cr ." [inc/dec/neg]" cr

  ." RAX inc-r: "
  reset-code RAX inc-r
  code-pos @ 3 = 0 byte@ $48 = and 1 byte@ $ff = and 2 byte@ $c0 = and
  check-result

  ." RAX dec-r: "
  reset-code RAX dec-r
  code-pos @ 3 = 0 byte@ $48 = and 1 byte@ $ff = and 2 byte@ $c8 = and
  check-result

  ." RAX neg-r: "
  reset-code RAX neg-r
  code-pos @ 3 = 0 byte@ $48 = and 1 byte@ $f7 = and 2 byte@ $d8 = and
  check-result

  ." R8 inc-r: "
  reset-code R8 inc-r
  code-pos @ 3 = 0 byte@ $49 = and 1 byte@ $ff = and 2 byte@ $c0 = and
  check-result
;

: test-shifts
  cr ." [shifts]" cr

  ." 4 RAX shl-ri: "
  reset-code 4 RAX shl-ri
  code-pos @ 4 = 0 byte@ $48 = and 1 byte@ $c1 = and 2 byte@ $e0 = and 3 byte@ $04 = and
  check-result

  ." 4 RBX shr-ri: "
  reset-code 4 RBX shr-ri
  code-pos @ 4 = 0 byte@ $48 = and 1 byte@ $c1 = and 2 byte@ $eb = and 3 byte@ $04 = and
  check-result

  ." 4 RCX sar-ri: "
  reset-code 4 RCX sar-ri
  code-pos @ 4 = 0 byte@ $48 = and 1 byte@ $c1 = and 2 byte@ $f9 = and 3 byte@ $04 = and
  check-result
;

: test-push-pop
  cr ." [push/pop]" cr

  ." RAX push-r: "
  reset-code RAX push-r
  code-pos @ 1 = 0 byte@ $50 = and
  check-result

  ." RBX push-r: "
  reset-code RBX push-r
  code-pos @ 1 = 0 byte@ $53 = and
  check-result

  ." R8 push-r: "
  reset-code R8 push-r
  code-pos @ 2 = 0 byte@ $41 = and 1 byte@ $50 = and
  check-result

  ." RAX pop-r: "
  reset-code RAX pop-r
  code-pos @ 1 = 0 byte@ $58 = and
  check-result

  ." RBX pop-r: "
  reset-code RBX pop-r
  code-pos @ 1 = 0 byte@ $5b = and
  check-result

  ." R8 pop-r: "
  reset-code R8 pop-r
  code-pos @ 2 = 0 byte@ $41 = and 1 byte@ $58 = and
  check-result
;

: test-special
  cr ." [syscall/ret/nop]" cr

  ." syscall: "
  reset-code syscall
  code-pos @ 2 = 0 byte@ $0f = and 1 byte@ $05 = and
  check-result

  ." ret: "
  reset-code ret
  code-pos @ 1 = 0 byte@ $c3 = and
  check-result

  ." nop: "
  reset-code nop
  code-pos @ 1 = 0 byte@ $90 = and
  check-result

  cr ." [xor-rr]" cr

  ." RAX RBX xor-rr: "
  reset-code RAX RBX xor-rr
  code-pos @ 3 = 0 byte@ $48 = and 1 byte@ $31 = and 2 byte@ $c3 = and
  check-result

  cr ." [cmp-rr]" cr

  ." RAX RBX cmp-rr: "
  reset-code RAX RBX cmp-rr
  code-pos @ 3 = 0 byte@ $48 = and 1 byte@ $39 = and 2 byte@ $c3 = and
  check-result

  cr ." [cqo]" cr

  ." cqo: "
  reset-code cqo
  code-pos @ 2 = 0 byte@ $48 = and 1 byte@ $99 = and
  check-result
;

: test-jumps
  cr ." [call/jmp]" cr

  ." $10 call-rel: "
  reset-code $10 call-rel
  code-pos @ 5 = 0 byte@ $e8 = and 1 byte@ $10 = and
  2 byte@ $00 = and 3 byte@ $00 = and 4 byte@ $00 = and
  check-result

  ." $20 jmp-rel: "
  reset-code $20 jmp-rel
  code-pos @ 5 = 0 byte@ $e9 = and 1 byte@ $20 = and
  2 byte@ $00 = and 3 byte@ $00 = and 4 byte@ $00 = and
  check-result

  ." $30 jz-rel: "
  reset-code $30 jz-rel
  code-pos @ 6 = 0 byte@ $0f = and 1 byte@ $84 = and 2 byte@ $30 = and
  3 byte@ $00 = and 4 byte@ $00 = and 5 byte@ $00 = and
  check-result

  ." $10 jz-rel8: "
  reset-code $10 jz-rel8
  code-pos @ 2 = 0 byte@ $74 = and 1 byte@ $10 = and
  check-result
;

: show-summary
  cr ." ===========================" cr
  ." TOTAL: " pass-count @ fail-count @ + .
  ."   PASS: " pass-count @ .
  ."   FAIL: " fail-count @ . cr

  fail-count @ 0= if
    ." All tests passed!" cr
  else
    ." Some tests failed." cr
  then
;

: run-tests
  cr ." === x86-64 Assembler Tests ===" cr cr
  test-mov-rr
  test-mov-ri
  test-add-sub
  test-unary
  test-shifts
  test-push-pop
  test-special
  test-jumps
  show-summary
;

run-tests
bye
