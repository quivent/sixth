\ defs.fs - Data Definitions (variable, constant, create, allot)
\ Ported from sixth.fs for Shannon Architecture
\
\ Requires: stack.fs, c,, d,, q,, code-here, dict-add

\ ============================================================
\ DATA SEGMENT
\ ============================================================
\ Data lives at 0x800000 in the ELF. data-here tracks allocation.

$800000 constant DATA-START
variable data-here  DATA-START data-here !

\ ============================================================
\ CURRENT WORD TRACKING (for recurse)
\ ============================================================

variable current-word-addr  0 current-word-addr !

\ ============================================================
\ RECURSE
\ ============================================================

: gen-recurse ( -- )
  \ Call current word being defined
  \ Save extra stack items before call (rbx, rcx)
  stack-depth @ 2 >= if $53 c, then   \ push rbx if NOS exists
  stack-depth @ 3 >= if $51 c, then   \ push rcx if 3rd exists
  \ Emit call
  current-word-addr @ code-here 5 + -
  $e8 c, d,                           \ call rel32
  \ Restore stack items after call
  stack-depth @ 3 >= if $59 c, then   \ pop rcx
  stack-depth @ 2 >= if $5b c, then ; \ pop rbx

\ ============================================================
\ VARIABLE
\ ============================================================
\ variable name - creates word that pushes address of 8-byte cell

: compile-variable ( -- )
  \ Parse name, create word that returns data address
  get-token dup 0= if 2drop exit then
  dict-add
  current-word-addr @ >r           \ save for nested definitions
  dict-buf dict-count @ 1- 32 * + dict-addr @ current-word-addr !
  \ Generate: mov rax, data-addr; ret
  push-val
  $48 c, $b8 c, data-here @ q,     \ mov rax, imm64
  ret
  8 data-here +!                   \ allocate 8 bytes
  r> current-word-addr ! ;

\ ============================================================
\ CONSTANT
\ ============================================================
\ n constant name - creates word that pushes n

variable const-val  0 const-val !

: compile-constant ( -- )
  \ Value should be on ct-stack from preceding number
  ct-depth@ 0= if
    ." constant requires value" cr 1 throw
  then
  ct-pop const-val !
  get-token dup 0= if 2drop exit then
  dict-add
  current-word-addr @ >r
  dict-buf dict-count @ 1- 32 * + dict-addr @ current-word-addr !
  \ Generate: mov rax, value; ret
  push-val
  $48 c, $b8 c, const-val @ q,     \ mov rax, imm64
  ret
  r> current-word-addr ! ;

\ ============================================================
\ CREATE
\ ============================================================
\ create name - creates word that pushes data-here address

: compile-create ( -- )
  get-token dup 0= if 2drop exit then
  dict-add
  current-word-addr @ >r
  dict-buf dict-count @ 1- 32 * + dict-addr @ current-word-addr !
  \ Generate: mov rax, data-addr; ret
  push-val
  $48 c, $b8 c, data-here @ q,     \ mov rax, imm64
  ret
  r> current-word-addr ! ;

\ ============================================================
\ ALLOT
\ ============================================================
\ n allot - allocate n bytes in data segment

: compile-allot ( -- )
  ct-depth@ 0= if
    ." allot requires size" cr 1 throw
  then
  ct-pop data-here +! ;

\ ============================================================
\ COMMA WORDS (, c,)
\ ============================================================
\ n , - store cell at data-here, advance
\ c c, - store byte at data-here, advance

: compile-data-comma ( -- )
  ct-depth@ 0= if
    ." , requires value" cr 1 throw
  then
  \ For now just advance data-here; actual storage needs init table
  ct-pop drop
  8 data-here +! ;

: compile-data-c-comma ( -- )
  ct-depth@ 0= if
    ." c, requires value" cr 1 throw
  then
  ct-pop drop
  1 data-here +! ;

