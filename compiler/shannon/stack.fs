\ stack.fs - Register-mapped stack machine (Shannon Layer 1)
\ Owns all stack-depth tracking and register allocation.
\
\ Register mapping:
\   TOS (top)   = RAX
\   NOS (next)  = RBX
\   3rd         = RCX
\   Deeper      = memory at [R15], R15 is stack pointer
\
\ Requires: asm.fs for register constants and instruction words
\ Requires: code-buf, code-pos, c, defined externally

\ Dependencies: asm.fs must be loaded before this file
\ require compiler/shannon/asm.fs

\ ============================================================
\ STATE (owned by this module)
\ ============================================================

variable stack-depth  0 stack-depth !
variable dead-code    0 dead-code !    \ 1 = after unconditional exit, skip depth tracking

\ ============================================================
\ PUBLIC INTERFACE - Register Access
\ ============================================================

: tos ( -- reg ) RAX ;      \ Current TOS register
: nos ( -- reg ) RBX ;      \ Current NOS register
: third ( -- reg ) RCX ;    \ Current 3rd register
: stkptr ( -- reg ) R15 ;   \ Memory stack pointer

\ ============================================================
\ PUBLIC INTERFACE - Depth Query/Set
\ ============================================================

: stack-depth@ ( -- n )
  \ Query current stack depth
  stack-depth @ ;

: stack-depth! ( n -- )
  \ Set depth (for control flow merge points)
  \ Respects dead-code flag
  dead-code @ if drop else stack-depth ! then ;

: stack-depth+! ( n -- )
  \ Adjust depth by n
  \ Respects dead-code flag
  dead-code @ if drop else stack-depth +! then ;

: dead-code@ ( -- flag ) dead-code @ ;
: dead-code! ( flag -- ) dead-code ! ;

\ ============================================================
\ INTERNAL - Memory Stack Operations
\ ============================================================

: spill-rcx ( -- )
  \ Push RCX to memory stack: sub r15, 8 ; mov [r15], rcx
  8 R15 sub-ri8
  RCX 0 R15 mov-mr ;

: fill-rcx ( -- )
  \ Pop RCX from memory stack: mov rcx, [r15] ; add r15, 8
  0 R15 RCX mov-rm
  8 R15 add-ri8 ;

\ ============================================================
\ PUBLIC INTERFACE - Stack Manipulation
\ ============================================================

: push-val ( -- )
  \ Make room for new TOS value (caller will set RAX)
  \ Cascades: rcx->mem, rbx->rcx, rax->rbx
  stack-depth @
  dup 3 >= if
    spill-rcx
  then
  dup 2 >= if
    RBX RCX mov-rr          \ mov rcx, rbx
  then
  drop
  RAX RBX mov-rr            \ mov rbx, rax
  1 stack-depth+! ;

: pop-val ( -- )
  \ Discard TOS, promote lower values
  \ Cascades: rbx->rax, rcx->rbx, mem->rcx
  RBX RAX mov-rr            \ mov rax, rbx
  stack-depth @ 1-
  dup 2 >= if
    RCX RBX mov-rr          \ mov rbx, rcx
  then
  dup 3 >= if
    fill-rcx
  then
  drop
  -1 stack-depth+! ;

: pop-nos-val ( -- )
  \ Discard NOS (keep TOS), promote lower values
  stack-depth @ 1-
  dup 2 >= if
    RCX RBX mov-rr          \ mov rbx, rcx
  then
  dup 3 >= if
    0 R15 RCX mov-rm        \ mov rcx, [r15]
    8 R15 R15 lea           \ lea r15, [r15+8]
  then
  drop
  -1 stack-depth+! ;

\ ============================================================
\ BACKWARD COMPATIBILITY ALIASES
\ ============================================================

: push-tos ( -- ) push-val ;
: pop-tos ( -- ) pop-val ;
: pop-nos ( -- ) pop-nos-val ;
: sdepth+! ( n -- ) stack-depth+! ;
: sdepth! ( n -- ) stack-depth! ;

\ ============================================================
\ PUBLIC INTERFACE - Literal Emission
\ ============================================================

: emit-lit ( n -- )
  \ Push literal n onto stack
  push-val
  dup 0= if
    drop
    RAX xor-rr-same         \ xor rax, rax (2 bytes)
  else dup $7FFFFFFF <= over $FFFFFFFF80000000 >= and if
    RAX mov-ri32            \ mov rax, imm32 (sign-extended, 7 bytes)
  else
    RAX mov-ri              \ mov rax, imm64 (10 bytes)
  then then ;

\ ============================================================
\ PUBLIC INTERFACE - Register Saves for Syscalls/Calls
\ ============================================================

: save-nos? ( -- flag )
  \ Should NOS be saved? (depth >= 2)
  stack-depth @ 2 >= ;

: save-third? ( -- flag )
  \ Should 3rd be saved? (depth >= 3)
  stack-depth @ 3 >= ;

: save-rbx ( -- )
  \ Push RBX to hardware stack if needed
  save-nos? if RBX push-r then ;

: restore-rbx ( -- )
  \ Pop RBX from hardware stack if needed
  save-nos? if RBX pop-r then ;

: save-rcx ( -- )
  \ Push RCX to hardware stack if needed
  save-third? if RCX push-r then ;

: restore-rcx ( -- )
  \ Pop RCX from hardware stack if needed
  save-third? if RCX pop-r then ;

: save-regs ( -- )
  \ Save caller-save registers (rbx, rcx) if live
  save-rbx save-rcx ;

: restore-regs ( -- )
  \ Restore caller-save registers (rcx, rbx) in reverse order
  restore-rcx restore-rbx ;
