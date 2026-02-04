\ sixth.fs - Forth to x86-64 native compiler
\ Stack: TOS=rax, NOS=rbx, 3rd=rcx, rest=memory at [r15]
\ Registers: r15=data stack, rbp=return stack, r12/r13=DO/LOOP
\ See ENCODING.md for full documentation

: >= ( a b -- flag ) < 0= ;
: <= ( a b -- flag ) > 0= ;
: <> ( a b -- flag ) = 0= ;

\ ============================================================
\ BUFFERS
\ ============================================================

262144 constant CODE-SIZE   \ 256KB for self-hosting
1024 constant DICT-SIZE     \ 1024 dictionary entries
150000 constant INPUT-SIZE

create code-buf CODE-SIZE allot
variable code-pos  0 code-pos !

create elf-buf 256 allot
variable elf-pos  0 elf-pos !

create input-buf INPUT-SIZE allot
variable input-len  0 input-len !
variable input-pos  0 input-pos !

\ Dictionary: 1024 entries, 32 bytes each (24 name + 4 addr + 4 flags)
create dict-buf DICT-SIZE 32 * allot
variable dict-count  0 dict-count !

\ Control flow stack for forward references
create cf-stack 64 cells allot
variable cf-sp  0 cf-sp !

\ Data segment allocation (variables, create/allot)
$800000 constant DATA-BASE    \ In separate data segment (was $40A000 in code)
\ Reserved data segment layout:
\   +0:   base variable (8 bytes, initialized to 10)
\   +8:   pno-pos variable (8 bytes)
\   +16:  pno-buf (128 bytes)
\   +144: source-buf (4096 bytes, input buffer)
\   +4240: >in variable (8 bytes, parse position)
\   +4248: source-len (8 bytes, input length)
\   +4256: word-buf (64 bytes, WORD result)
\   +4320: user data starts here
DATA-BASE constant rt-base         \ runtime address of base variable
DATA-BASE 8 + constant rt-pno-pos  \ runtime address of pno position
DATA-BASE 16 + constant rt-pno-buf \ runtime address of PNO buffer (128 bytes)
128 constant PNO-SIZE
DATA-BASE 144 + constant rt-source-buf   \ runtime address of input buffer
4096 constant SOURCE-SIZE
DATA-BASE 4240 + constant rt->in         \ runtime address of >in
DATA-BASE 4248 + constant rt-source-len  \ runtime address of source length
DATA-BASE 4256 + constant rt-word-buf    \ runtime address of WORD buffer
64 constant WORD-SIZE

\ Runtime dictionary for FIND
\ Entry: name[24] + xt[8] + flags[8] = 40 bytes
DATA-BASE 4320 + constant rt-dict-count   \ 8 bytes: number of entries
DATA-BASE 4328 + constant rt-dict-buf     \ 256 * 40 = 10240 bytes
40 constant RT-DICT-ENTRY-SIZE
1024 constant RT-DICT-MAX
DATA-BASE 45288 + constant rt-state       \ runtime STATE: 0=interpret, 1=compile
DATA-BASE 45296 + constant rt-source-addr \ pointer to current input buffer
DATA-BASE 45304 + constant rt-last-create \ address of last CREATE'd word's code
DATA-BASE 45312 + constant rt-argc        \ command line argument count
DATA-BASE 45320 + constant rt-argv        \ pointer to argv array
DATA-BASE 45328 + constant rt-slurp-buf   \ slurp-file buffer (262144 bytes)
262144 constant SLURP-SIZE

variable data-here  DATA-BASE 45328 SLURP-SIZE + + data-here !

\ Data initialization table (for , and c,)
4096 constant INIT-MAX
create init-buf INIT-MAX 24 * allot  \ (addr:8, value:8, size:8)
variable init-count  0 init-count !
: init-entry ( i -- addr ) 24 * init-buf + ;
: add-init ( value addr size -- )
  init-count @ INIT-MAX >= if ." Too many , initializations" cr 1 throw then
  init-count @ init-entry >r
  r@ 16 + !  r@ !  r> 8 + !
  1 init-count +! ;

\ Compilation state
variable state  0 state !   \ 0=interpret, 1=compile
variable interp-val  0 interp-val !
variable interp-val2 0 interp-val2 !  \ second slot for interpret mode
variable rt-find-addr  0 rt-find-addr !  \ Runtime FIND function address
variable tos-cached  1 tos-cached !  \ Track if TOS is in rax
variable full-source  0 full-source !  \ 0=fast (direct rt-source-buf), 1=full (indirect rt-source-addr)
variable last-create-code  0 last-create-code !  \ code-pos of last CREATE (for DOES>)
variable last-create-dict  0 last-create-dict !  \ dict index of last CREATE (for DOES>)

\ OPTIMIZATION 6: REGISTER-BASED STACK
\ Stack depth: 0=empty, 1=rax, 2=rax+rbx, 3=rax+rbx+rcx, 4+=memory
\ Shallow stacks (depth <= 3) live entirely in registers, no memory traffic.
variable stack-depth  0 stack-depth !

\ OPTIMIZATION 4: DEAD CODE ELIMINATION - track purity and stack effect
variable has-io       0 has-io !        \ does current word have I/O?
variable start-depth  0 start-depth !   \ stack depth at word entry
variable arg-count    1 arg-count !     \ number of input arguments (from stack comment)

\ Pending call elimination
variable pending-call   0 pending-call !   \ address of pending call, 0=none
variable do-depth       0 do-depth !       \ nesting depth of do/loop (for exit cleanup)
create do-trip 8 cells allot              \ known trip count per nesting level (-1=unknown)
create do-origin 8 cells allot            \ code-here before gen-do emitted anything
create do-sdepth 8 cells allot            \ stack-depth before gen-do
create do-leave 8 cells allot             \ leave patch address per level (0=none)

\ Compile-time constant stack for literal folding/fusion
create ct-stack 8 cells allot
variable ct-depth  0 ct-depth !

\ Word info table for double-pass (Pass 1 scan)
512 constant INFO-MAX
\ info-buf entry (40 bytes):
\   0-23:  name (24 bytes)
\   24:    nargs (1 byte)
\   25:    rets (1 byte)
\   26-27: flags (2 bytes) - bit0=recursive, bit1=has-io
\   28-29: call-count (2 bytes)
\   30-33: body-pos (4 bytes) - source position where body starts
\   34-37: code-addr (4 bytes) - code offset, filled when compiled (0 = not compiled)
\   38-39: padding
create info-buf INFO-MAX 40 * allot
variable info-count  0 info-count !

\ ============================================================
\ CODE EMISSION
\ ============================================================

: c, ( b -- ) code-buf code-pos @ + c!  1 code-pos +! ;
: d, ( d -- ) dup c, 8 rshift dup c, 8 rshift dup c, 8 rshift c, ;
: q, ( q -- ) dup d, 32 rshift d, ;

: code-here ( -- addr ) code-pos @ ;
: patch-rel32 ( target from -- )
  dup >r  4 + -
  r> code-buf +
  2dup c!
  swap 8 rshift swap 1+ 2dup c!
  swap 8 rshift swap 1+ 2dup c!
  swap 8 rshift swap 1+ c! ;

\ ============================================================
\ ELF EMISSION
\ ============================================================

: e, ( b -- ) elf-buf elf-pos @ + c!  1 elf-pos +! ;
: e2, ( w -- ) dup e, 8 rshift e, ;
: e4, ( d -- ) dup e2, 16 rshift e2, ;
: e8, ( q -- ) dup e4, 32 rshift e4, ;

: elf-header ( -- )
  0 elf-pos !
  $7f e, 69 e, 76 e, 70 e,  2 e, 1 e, 1 e, 0 e,  0 e8,
  2 e2, $3e e2, 1 e4,  $4000B0 e8,  64 e8, 0 e8, 0 e4,
  64 e2, 56 e2, 2 e2,  0 e2, 0 e2, 0 e2,
  \ code RX
  1 e4, 5 e4, 0 e8,  $400000 e8, $400000 e8,
  176 code-pos @ + dup e8, e8,  $1000 e8,
  \ data RW
  1 e4, 6 e4, 0 e8,  $800000 e8, $800000 e8,
  0 e8, $200000 e8,  $1000 e8, ;

: write-elf ( addr u -- )
  w/o create-file throw >r
  elf-buf 176 r@ write-file throw
  code-buf code-pos @ r@ write-file throw
  r> close-file throw ;

\ ============================================================
\ CONTROL FLOW STACK
\ ============================================================

: cf-push ( n -- )
  cf-sp @ 64 >= if ." CF OVERFLOW!" cr 1 throw then
  dup -100 > over 0< and if ." CF-PUSH NEG: " dup . ." cfsp=" cf-sp @ . ." cp=" code-pos @ . cr then
  cf-stack cf-sp @ cells + !  1 cf-sp +! ;
: cf-pop ( -- n )
  cf-sp @ 0= if ." CF UNDERFLOW!" cr 1 throw then
  -1 cf-sp +!  cf-stack cf-sp @ cells + @ ;

\ ============================================================
\ DICTIONARY
\ ============================================================

: dict-entry ( -- addr ) dict-buf dict-count @ 32 * + ;
: dict-name ( entry -- addr ) ;
: dict-addr ( entry -- addr ) 24 + ;
: dict-flags ( entry -- addr ) 28 + ;

\ INLINING: DISABLED
\ The compiler's dup-pending optimization breaks inlining:
\ "dup *" compiles to imul rax,rbx assuming rbx=dup of rax.
\ When inlined into a context with something else in rbx, wrong result.
\ Would need to track which words use dup-pending and exclude them.

32 constant FIXUP-MAX
create fixup-buf FIXUP-MAX 28 * allot
variable fixup-count  0 fixup-count !

: fixup-entry ( i -- addr ) 28 * fixup-buf + ;
: fixup-name= ( addr u entry -- flag )
  >r dup 23 > if 2drop r> drop false exit then
  r@ over + c@ 0 <> if r> drop 2drop false exit then
  r> swap 0 ?do
    2dup i + c@ swap i + c@ <> if 2drop false unloop exit then
  loop 2drop true ;

variable fixup-patch  0 fixup-patch !
variable fixup-entry-p  0 fixup-entry-p !
: add-fixup ( name-addr name-u patch-from -- )
  fixup-count @ FIXUP-MAX >= if ." Too many forward refs" cr 1 throw then
  fixup-patch !
  fixup-count @ fixup-entry fixup-entry-p !
  fixup-entry-p @ 24 0 fill
  fixup-patch @ fixup-entry-p @ 24 + !
  dup 23 > if drop 23 then
  fixup-entry-p @ swap move
  1 fixup-count +! ;

variable fixup-target  0 fixup-target !
: resolve-fixups ( addr u code-addr -- )
  fixup-target ! fixup-count @ 0 ?do
    2dup i fixup-entry fixup-name= if
      fixup-target @ i fixup-entry 24 + @ 4 - patch-rel32
      i fixup-entry 24 0 fill
    then
  loop 2drop ;

: dict-add ( addr u -- )
  dict-count @ DICT-SIZE >= if 2drop exit then
  2dup code-here resolve-fixups
  dict-entry >r
  r@ 24 0 fill
  dup 23 > if drop 23 then
  r@ swap move
  code-here r@ dict-addr !
  0 r> dict-flags !
  1 dict-count +! ;

: dict-name= ( addr u entry -- flag )
  >r
  dup 23 > if 2drop r> drop false exit then
  r@ over + c@ 0 <> if r> drop 2drop false exit then
  r> swap 0 ?do
    2dup i + c@ swap i + c@ <> if 2drop false unloop exit then
  loop
  2drop true ;

: dict-find ( addr u -- entry | 0 )
  dict-count @ 0 ?do
    2dup dict-buf i 32 * + dict-name=
    if 2drop dict-buf i 32 * + unloop exit then
  loop
  2drop 0 ;

: immediate ( -- )
  dict-count @ 0> if
    1 dict-buf dict-count @ 1- 32 * + dict-flags !
  then ;

\ ============================================================
\ TOS MANAGEMENT (rax = TOS, r15 = stack pointer)
\ ============================================================

: push-tos ( -- )
  stack-depth @
  dup 3 >= if
    $49 c, $83 c, $ef c, 8 c,       \ sub r15, 8
    $49 c, $89 c, $0f c,            \ mov [r15], rcx
  then
  dup 2 >= if
    $48 c, $89 c, $d9 c,            \ mov rcx, rbx
  then
  drop
  $48 c, $89 c, $c3 c,              \ mov rbx, rax
  1 stack-depth +! ;

: pop-tos ( -- )
  $48 c, $89 c, $d8 c,
  stack-depth @ 1- dup
  2 >= if
    $48 c, $89 c, $cb c,
  then
  3 >= if
    $49 c, $8b c, $0f c,
    $49 c, $83 c, $c7 c, 8 c,
  then
  -1 stack-depth +! ;

: pop-nos ( -- )
  stack-depth @ 1- dup
  2 >= if
    $48 c, $89 c, $cb c,
  then
  3 >= if
    $49 c, $8b c, $0f c,
    $4d c, $8d c, $7f c, 8 c,
  then
  -1 stack-depth +! ;

\ ============================================================
\ CODE GENERATION - PRIMITIVES
\ ============================================================

: gen-lit ( n -- )
  push-tos
  dup 0= if
    drop
    $31 c, $c0 c,
  else dup $7FFFFFFF <= over $FFFFFFFF80000000 >= and if
    $48 c, $c7 c, $c0 c, d,
  else
    $48 c, $b8 c, q,
  then then ;

\ ============================================================
\ COMPILE-TIME CONSTANT STACK
\ ============================================================
\ OPTIMIZATION 1: CONSTANT FOLDING infrastructure.
\ Literals are pushed here instead of emitting code. When an
\ arithmetic op follows, constants fold at compile time.
\ ct-flush emits all pending constants (FIFO) when a non-foldable
\ word is encountered (stack ops, control flow, I/O, calls).
\ Tests: 1000-fold-add through 1049-fold-no-runtime-leak

: ct-push ( n -- )    ct-stack ct-depth @ cells + !  1 ct-depth +! ;
: ct-pop ( -- n )     -1 ct-depth +!  ct-stack ct-depth @ cells + @ ;
: ct-flush ( -- )
  ct-depth @ 0= if exit then
  ct-depth @ 0 do
    ct-stack i cells + @ gen-lit
  loop
  0 ct-depth ! ;

: gen-dup ( -- )  push-tos ;

: gen-dup2 ( -- )
  $49 c, $83 c, $ef c, 16 c,
  $49 c, $89 c, $07 c,
  $49 c, $89 c, $47 c, 8 c, ;

: gen-drop ( -- )  pop-tos ;

: gen-swap ( -- )  $48 c, $87 c, $c3 c, ;

: gen-over ( -- )
  push-tos
  $48 c, $89 c, $c8 c, ;

: gen-rot ( -- )
  $48 c, $87 c, $c1 c,
  $48 c, $87 c, $cb c, ;

: gen-nip ( -- )  pop-nos ;

: gen-tuck ( -- )  gen-swap gen-over ;

: gen-2dup ( -- )
  push-tos
  $48 c, $89 c, $c8 c,
  push-tos
  $48 c, $89 c, $c8 c, ;

: gen-2drop ( -- )  pop-tos pop-tos ;

: gen--rot ( -- )  gen-rot gen-rot ;

: gen-?dup ( -- )
  \ ( x -- x x | 0 ) test rax; if nonzero, dup
  $48 c, $85 c, $c0 c,           \ test rax, rax
  $74 c,                          \ jz (skip push-tos)
  code-here 0 c,                  \ save offset addr, emit placeholder
  push-tos
  code-here over - 1-             \ skip distance = bytes emitted by push-tos
  swap code-buf + c! ;            \ patch jz offset

: gen-2swap ( -- )
  \ ( a b c d -- c d a b ) d=rax, c=rbx, b=rcx, a=[r15]
  $48 c, $87 c, $c1 c,           \ xchg rax, rcx  (rax=b, rcx=d)
  $49 c, $87 c, $1f c, ;         \ xchg rbx, [r15] (rbx=a, [r15]=c)

: gen-2over ( -- )
  \ ( a b c d -- a b c d a b ) d=rax, c=rbx, b=rcx, a=[r15]
  \ Save b (rcx) and a ([r15]) to x86 stack before registers shift
  $51 c,                          \ push rcx (save b)
  $49 c, $ff c, $37 c,           \ push [r15] (save a)
  push-tos                        \ make room for first value
  $58 c,                          \ pop rax (a)
  push-tos                        \ make room for second value
  $58 c, ;                        \ pop rax (b)

: gen-depth ( -- )
  \ Push current stack depth as a value (subtract 1 for phantom TOS)
  stack-depth @ 1- dup           \ actual depth = tracker - 1
  push-tos
  0= if drop $31 c, $c0 c,       \ xor eax, eax (depth = 0)
  else $48 c, $c7 c, $c0 c, d,   \ mov rax, imm32
  then ;

: gen-pick ( -- )
  \ ( xu...x1 x0 u -- xu...x1 x0 xu )
  \ u is in rax. x0 is in rbx, x1 in rcx, x2+ in [r15...]
  \ u=0 → get rbx, u=1 → get rcx, u>=2 → get [r15 + (u-2)*8]
  $48 c, $89 c, $c7 c,           \ mov rdi, rax (save index)
  $48 c, $83 c, $ff c, 2 c,      \ cmp rdi, 2
  $7c c, 19 c,                   \ jl +19 (skip 19 bytes of memory case)
  \ Memory case: [r15 + (rdi-2)*8] — 19 bytes total
  $48 c, $83 c, $ef c, 2 c,      \ sub rdi, 2 (4)
  $48 c, $c1 c, $e7 c, 3 c,      \ shl rdi, 3 (4)
  $49 c, $01 c, $ff c,           \ add r15, rdi (3)
  $49 c, $8b c, $07 c,           \ mov rax, [r15] (3)
  $49 c, $29 c, $ff c,           \ sub r15, rdi (3)
  $eb c, 13 c,                   \ jmp done, skip 13 bytes (2)
  \ Register case (u=0 or u=1): — 13 bytes total
  $48 c, $85 c, $ff c,           \ test rdi, rdi (3)
  $75 c, 5 c,                    \ jnz +5 (2)
  $48 c, $89 c, $d8 c,           \ mov rax, rbx (u=0: x0) (3)
  $eb c, 3 c,                    \ jmp done (2)
  $48 c, $89 c, $c8 c, ;         \ mov rax, rcx (u=1: x1) (3)

: gen-space ( -- )
  \ emit space character via write syscall
  1 has-io !
  stack-depth @ 2 >= if $53 c, then
  stack-depth @ 3 >= if $51 c, then
  $6a c, 32 c,                   \ push 32
  $b8 c, 1 d,                    \ mov eax, 1 (sys_write)
  $bf c, 1 d,                    \ mov edi, 1 (stdout)
  $48 c, $89 c, $e6 c,           \ mov rsi, rsp
  $ba c, 1 d,                    \ mov edx, 1
  $0f c, $05 c,                  \ syscall
  $58 c,                         \ pop scratch
  stack-depth @ 3 >= if $59 c, then
  stack-depth @ 2 >= if $5b c, then ;

: gen-spaces ( -- )
  \ ( n -- ) emit n spaces. Uses loop with syscall.
  1 has-io !
  stack-depth @ 2 >= if $53 c, then       \ save rbx
  stack-depth @ 3 >= if $51 c, then       \ save rcx
  \ rax = count. Loop: test, jle done, emit space, dec, jmp back.
  code-here                                \ ( loop-start )
  $48 c, $85 c, $c0 c,                    \ test rax, rax
  $7e c,                                   \ jle done (short, patch later)
  code-here 0 c,                           \ ( loop-start patch-pos ) placeholder
  $50 c,                                   \ push rax (save counter)
  $6a c, 32 c,                            \ push 32
  $b8 c, 1 d,                             \ mov eax, 1
  $bf c, 1 d,                             \ mov edi, 1
  $48 c, $89 c, $e6 c,                    \ mov rsi, rsp
  $ba c, 1 d,                             \ mov edx, 1
  $0f c, $05 c,                            \ syscall
  $58 c,                                   \ pop scratch (the space)
  $58 c,                                   \ pop rax (restore counter)
  $48 c, $ff c, $c8 c,                    \ dec rax
  $eb c,                                   \ jmp loop-start (short)
  code-here 1+ rot - negate c,             \ relative offset back to test
  \ patch the jle forward jump
  code-here over - 1- swap code-buf + c!   \ patch jle offset
  stack-depth @ 3 >= if $59 c, then       \ restore rcx
  stack-depth @ 2 >= if $5b c, then       \ restore rbx
  pop-tos ;

: gen-key ( -- )
  \ ( -- char ) read one byte from stdin via syscall
  1 has-io !
  push-tos
  stack-depth @ 2 >= if $53 c, then       \ save rbx
  stack-depth @ 3 >= if $51 c, then       \ save rcx
  $6a c, 0 c,                             \ push 0 (buffer on stack)
  $48 c, $31 c, $c0 c,                    \ xor eax, eax (sys_read=0)
  $48 c, $31 c, $ff c,                    \ xor edi, edi (stdin=0)
  $48 c, $89 c, $e6 c,                    \ mov rsi, rsp (buf=stack)
  $ba c, 1 d,                             \ mov edx, 1
  $0f c, $05 c,                            \ syscall
  $58 c,                                   \ pop rax (the byte, zero-extended)
  $48 c, $0f c, $b6 c, $c0 c,            \ movzx rax, al
  stack-depth @ 3 >= if $59 c, then       \ restore rcx
  stack-depth @ 2 >= if $5b c, then ;     \ restore rbx

: gen-move ( -- )
  \ ( src dst u -- ) u=rax, dst=rbx, src=rcx or [r15]
  \ rsi/rdi are free (not used by stack machine)
  stack-depth @ 3 >= if
    $48 c, $89 c, $ce c,         \ mov rsi, rcx (src from 3rd reg)
  else
    $49 c, $8b c, $37 c,         \ mov rsi, [r15]
    $49 c, $83 c, $c7 c, 8 c,   \ add r15, 8
  then
  $48 c, $89 c, $df c,           \ mov rdi, rbx (dst)
  $48 c, $89 c, $c1 c,           \ mov rcx, rax (count)
  $f3 c, $a4 c,                  \ rep movsb
  \ Promote remaining items into registers
  stack-depth @ 4 >= if
    $49 c, $8b c, $07 c,           \ mov rax, [r15]
    stack-depth @ 5 >= if
      $49 c, $8b c, $5f c, 8 c,    \ mov rbx, [r15+8]
      stack-depth @ 6 >= if
        $49 c, $8b c, $4f c, 16 c,   \ mov rcx, [r15+16]
      then
    then
    stack-depth @ 3 - dup 3 > if drop 3 then 8 *
    dup 0> if
      $49 c, $83 c, $c7 c, c,      \ add r15, N*8
    else drop then
  then
  -3 stack-depth +! ;

: gen-fill ( -- )
  \ ( addr u char -- ) char=rax, u=rbx, addr=rcx or [r15]
  \ rdi is free
  stack-depth @ 3 >= if
    $48 c, $89 c, $cf c,         \ mov rdi, rcx (addr from 3rd reg)
  else
    $49 c, $8b c, $3f c,         \ mov rdi, [r15]
    $49 c, $83 c, $c7 c, 8 c,   \ add r15, 8
  then
  $48 c, $89 c, $d9 c,           \ mov rcx, rbx (count)
  \ rax already has char (al used by stosb)
  $f3 c, $aa c,                  \ rep stosb
  \ Promote remaining items (same pattern as move)
  stack-depth @ 4 >= if
    $49 c, $8b c, $07 c,
    stack-depth @ 5 >= if
      $49 c, $8b c, $5f c, 8 c,
      stack-depth @ 6 >= if
        $49 c, $8b c, $4f c, 16 c,
      then
    then
    stack-depth @ 3 - dup 3 > if drop 3 then 8 *
    dup 0> if
      $49 c, $83 c, $c7 c, c,
    else drop then
  then
  -3 stack-depth +! ;

\ ============================================================
\ ARITHMETIC
\ ============================================================

: gen-add ( -- )  $48 c, $01 c, $d8 c,  pop-nos ;

: gen-sub ( -- )
  $48 c, $29 c, $c3 c,
  $48 c, $89 c, $d8 c,
  pop-nos ;

\ reverse subtract: TOS - NOS (swap sub optimization)
: gen-rsub ( -- )  $48 c, $29 c, $d8 c,  pop-nos ;

: gen-mul ( -- )  $48 c, $0f c, $af c, $c3 c,  pop-nos ;

: gen-div ( -- )
  stack-depth @ 3 >= if $51 c, then
  $48 c, $89 c, $c7 c,
  $48 c, $89 c, $d8 c,
  $48 c, $99 c,
  $48 c, $f7 c, $ff c,
  stack-depth @ 3 >= if $59 c, then
  pop-nos ;

: gen-mod ( -- )
  stack-depth @ 3 >= if $51 c, then
  $48 c, $89 c, $c7 c,
  $48 c, $89 c, $d8 c,
  $48 c, $99 c,
  $48 c, $f7 c, $ff c,
  $48 c, $89 c, $d0 c,
  stack-depth @ 3 >= if $59 c, then
  pop-nos ;

: gen-/mod ( -- )
  \ ( n1 n2 -- rem quot ) same as gen-div but keep both results
  stack-depth @ 3 >= if $51 c, then       \ push rcx (save 3rd)
  $48 c, $89 c, $c7 c,                    \ mov rdi, rax (divisor)
  $48 c, $89 c, $d8 c,                    \ mov rax, rbx (dividend)
  $48 c, $99 c,                            \ cqo (sign-extend rax → rdx:rax)
  $48 c, $f7 c, $ff c,                    \ idiv rdi → rax=quot, rdx=rem
  $48 c, $89 c, $d3 c,                    \ mov rbx, rdx (rem → NOS)
  stack-depth @ 3 >= if $59 c, then ;     \ pop rcx (restore 3rd)
  \ stack unchanged: NOS=rem, TOS=quot

: gen-*/mod ( -- )
  \ ( n1 n2 n3 -- rem quot ) n1*n2/n3 with intermediate double
  \ n3=TOS=rax, n2=NOS=rbx, n1=3rd (rcx or [r15])
  $50 c,                                   \ push rax (save n3=divisor)
  stack-depth @ 3 >= if
    $48 c, $89 c, $c8 c,                  \ mov rax, rcx (n1 from 3rd reg)
  else
    $49 c, $8b c, $07 c,                  \ mov rax, [r15] (n1 from memory)
    $49 c, $83 c, $c7 c, 8 c,            \ add r15, 8
  then
  $48 c, $f7 c, $eb c,                    \ imul rbx → rdx:rax = n1*n2
  $59 c,                                   \ pop rcx (n3=divisor)
  $48 c, $f7 c, $f9 c,                    \ idiv rcx → rax=quot, rdx=rem
  $48 c, $89 c, $d3 c,                    \ mov rbx, rdx (rem → NOS)
  -1 stack-depth +! ;                      \ consumed 3, produced 2

: gen-*/ ( -- )
  \ ( n1 n2 n3 -- quot ) n1*n2/n3 with intermediate double
  \ Same as */mod but discard remainder
  $50 c,                                   \ push rax (save n3=divisor)
  stack-depth @ 3 >= if
    $48 c, $89 c, $c8 c,                  \ mov rax, rcx (n1 from 3rd reg)
  else
    $49 c, $8b c, $07 c,                  \ mov rax, [r15] (n1 from memory)
    $49 c, $83 c, $c7 c, 8 c,            \ add r15, 8
  then
  $48 c, $f7 c, $eb c,                    \ imul rbx → rdx:rax = n1*n2
  $59 c,                                   \ pop rcx (n3=divisor)
  $48 c, $f7 c, $f9 c,                    \ idiv rcx → rax=quot, rdx=rem
  pop-nos                                  \ drop NOS, consumed 3 produced 1
  -1 stack-depth +! ;

: gen-negate ( -- )  $48 c, $f7 c, $d8 c, ;
: gen-1+ ( -- )  $48 c, $ff c, $c0 c, ;
: gen-1- ( -- )  $48 c, $ff c, $c8 c, ;
: gen-nos+ ( -- )  $48 c, $ff c, $c3 c, ;

variable swap-pending  0 swap-pending !

\ OPTIMIZATION 8: dup + comparison + while/until fusion (see header)
\ DO NOT REMOVE. Removing breaks all begin/while/repeat and begin/until loops.
\ cmp-pending: 0=none, 1=0=, 2=0>, 3=0<
variable dup-pending   0 dup-pending !
variable cmp-pending   0 cmp-pending !

: flush-cmp ( -- )
  cmp-pending @ ?dup if
    gen-dup  \ the deferred dup
    dup 1 = if  \ 0= inline
      $48 c, $85 c, $c0 c, $0f c, $94 c, $c0 c,
      $48 c, $0f c, $b6 c, $c0 c, $48 c, $f7 c, $d8 c, then
    dup 2 = if  \ 0> inline
      $48 c, $85 c, $c0 c, $0f c, $9f c, $c0 c,
      $48 c, $0f c, $b6 c, $c0 c, $48 c, $f7 c, $d8 c, then
        3 = if  \ 0< inline
      $48 c, $c1 c, $f8 c, 63 c, then
    0 cmp-pending !
  then
  dup-pending @ if gen-dup  0 dup-pending ! then ;

: flush-swap ( -- )
  flush-cmp
  swap-pending @ if gen-swap  0 swap-pending ! then ;

: gen-2+ ( -- )  $48 c, $83 c, $c0 c, 2 c, ;
: gen-2- ( -- )  $48 c, $83 c, $e8 c, 2 c, ;
: gen-tuck+ ( -- )  $48 c, $0f c, $c1 c, $d8 c, ;

: gen-and ( -- )  $48 c, $21 c, $d8 c,  pop-nos ;
: gen-or ( -- )   $48 c, $09 c, $d8 c,  pop-nos ;
: gen-xor ( -- )  $48 c, $31 c, $d8 c,  pop-nos ;
: gen-invert ( -- )  $48 c, $f7 c, $d0 c, ;

: gen-abs ( -- )
  $48 c, $89 c, $c7 c,
  $48 c, $f7 c, $d8 c,
  $48 c, $0f c, $48 c, $c7 c, ;

: gen-2* ( -- )  $48 c, $d1 c, $e0 c, ;
: gen-2/ ( -- )  $48 c, $d1 c, $f8 c, ;

\ ============================================================
\ FUSED IMMEDIATE ARITHMETIC (constant + runtime op)
\ ============================================================
\ OPTIMIZATION 2: LITERAL-OP FUSION codegen.
\ When ct-depth=1 and a binary op follows, emit a single x86
\ instruction with an immediate operand instead of push+op+pop.
\ e.g. gen-add-imm emits "add rax, imm32" (3 bytes vs ~12).
\ This is the key optimization for the arith benchmark:
\   3 * 7 + $FFFFFF and → imul+add+and (3 insns, not ~21).
\ Tests: 1016-fuse-add-imm through 1021-fuse-xor-imm, 1034, 1043, 1047

: gen-add-imm ( n -- )  $48 c, $05 c, d, ;
: gen-sub-imm ( n -- )  $48 c, $2d c, d, ;
: gen-mul-imm ( n -- )  $48 c, $69 c, $c0 c, d, ;
: gen-and-imm ( n -- )  $48 c, $25 c, d, ;
: gen-or-imm  ( n -- )  $48 c, $0d c, d, ;
: gen-xor-imm ( n -- )  $48 c, $35 c, d, ;

\ ============================================================
\ STRENGTH REDUCTION - multiply/modulo by powers of 2
\ ============================================================

: power-of-2? ( n -- flag )
  dup 1 < if drop false exit then   \ n <= 0 means not power of 2
  dup 1- and 0= ;

: log2 ( n -- shift )
  0 swap begin dup 1 > while 1 rshift swap 1+ swap repeat drop ;

: gen-lshift-imm ( n -- )
  dup 1 = if drop $48 c, $01 c, $c0 c,      \ add rax, rax
  else $48 c, $c1 c, $e0 c, c, then ;       \ shl rax, N

\ ============================================================
\ COMPARISON (result: 0 or -1)
\ ============================================================

: gen-cmp-setup ( -- )
  $48 c, $39 c, $c3 c,
  pop-nos ;

: gen-= ( -- )
  gen-cmp-setup
  $0f c, $94 c, $c0 c,
  $48 c, $0f c, $b6 c, $c0 c,
  $48 c, $f7 c, $d8 c, ;

: gen-<> ( -- )
  gen-cmp-setup
  $0f c, $95 c, $c0 c,
  $48 c, $0f c, $b6 c, $c0 c,
  $48 c, $f7 c, $d8 c, ;

: gen-< ( -- )
  gen-cmp-setup
  $0f c, $9c c, $c0 c,
  $48 c, $0f c, $b6 c, $c0 c,
  $48 c, $f7 c, $d8 c, ;

: gen-> ( -- )
  gen-cmp-setup
  $0f c, $9f c, $c0 c,
  $48 c, $0f c, $b6 c, $c0 c,
  $48 c, $f7 c, $d8 c, ;

: gen-<= ( -- )
  gen-cmp-setup
  $0f c, $9e c, $c0 c,
  $48 c, $0f c, $b6 c, $c0 c,
  $48 c, $f7 c, $d8 c, ;

: gen->= ( -- )
  gen-cmp-setup
  $0f c, $9d c, $c0 c,
  $48 c, $0f c, $b6 c, $c0 c,
  $48 c, $f7 c, $d8 c, ;

: gen-u< ( -- )
  \ ( u1 u2 -- flag ) unsigned less-than
  gen-cmp-setup                            \ cmp rbx, rax; pop-nos
  $0f c, $92 c, $c0 c,                    \ setb al (unsigned below)
  $48 c, $0f c, $b6 c, $c0 c,            \ movzx rax, al
  $48 c, $f7 c, $d8 c, ;                  \ neg rax (true = -1)

: gen-0= ( -- )
  $48 c, $85 c, $c0 c,
  $0f c, $94 c, $c0 c,
  $48 c, $0f c, $b6 c, $c0 c,
  $48 c, $f7 c, $d8 c, ;

: gen-0< ( -- )
  $48 c, $c1 c, $f8 c, 63 c, ;

: gen-0> ( -- )
  $48 c, $85 c, $c0 c,
  $0f c, $9f c, $c0 c,
  $48 c, $0f c, $b6 c, $c0 c,
  $48 c, $f7 c, $d8 c, ;

: gen-0<> ( -- )
  \ ( n -- flag ) true if n != 0
  $48 c, $85 c, $c0 c,           \ test rax, rax
  $0f c, $95 c, $c0 c,           \ setne al
  $48 c, $0f c, $b6 c, $c0 c,   \ movzx rax, al
  $48 c, $f7 c, $d8 c, ;        \ neg rax

: gen-within ( -- )
  \ ( u lo hi -- flag ) true if lo <= u < hi
  \ Equivalent to: over - >r - r> u<
  \ hi=rax, lo=rbx, u=rcx or [r15]
  stack-depth @ 3 >= if
    $48 c, $89 c, $cf c,           \ mov rdi, rcx (u)
  else
    $49 c, $8b c, $3f c,           \ mov rdi, [r15]
    $49 c, $83 c, $c7 c, 8 c,     \ add r15, 8
  then
  $48 c, $29 c, $d8 c,             \ sub rax, rbx (hi - lo)
  $48 c, $29 c, $df c,             \ sub rdi, rbx (u - lo)
  $48 c, $39 c, $c7 c,             \ cmp rdi, rax
  $0f c, $92 c, $c0 c,             \ setb al
  $48 c, $0f c, $b6 c, $c0 c,     \ movzx rax, al
  $48 c, $f7 c, $d8 c,             \ neg rax
  -2 stack-depth +! ;

: gen-count ( -- )
  \ ( c-addr -- c-addr+1 u ) get length byte, advance address
  push-tos
  $48 c, $0f c, $b6 c, $03 c,     \ movzx rax, byte [rbx]
  $48 c, $ff c, $c3 c, ;          \ inc rbx

: gen-min ( -- )
  \ ( a b -- min ) b=rax, a=rbx. cmp rbx,rax; cmovl rax,rbx
  $48 c, $39 c, $c3 c,
  $48 c, $0f c, $4c c, $c3 c,
  pop-nos ;

: gen-max ( -- )
  \ ( a b -- max ) b=rax, a=rbx. cmp rbx,rax; cmovg rax,rbx
  $48 c, $39 c, $c3 c,
  $48 c, $0f c, $4f c, $c3 c,
  pop-nos ;

: gen-lshift ( -- )
  \ ( value count -- result ) count=rax, value=rbx
  \ Save count to cl via x86 stack, then use pop-nos pattern
  $51 c,                                \ push rcx (save 3rd item)
  $48 c, $89 c, $c1 c,                 \ mov rcx, rax (count → cl)
  $48 c, $89 c, $d8 c,                 \ mov rax, rbx (value → rax)
  $48 c, $d3 c, $e0 c,                 \ shl rax, cl
  $59 c,                                \ pop rcx (restore 3rd item)
  \ Now promote: rbx=rcx, rcx=mem if needed
  stack-depth @ 1- dup
  2 >= if
    $48 c, $89 c, $cb c,               \ mov rbx, rcx
  then
  3 >= if
    $49 c, $8b c, $0f c,               \ mov rcx, [r15]
    $49 c, $83 c, $c7 c, 8 c,          \ add r15, 8
  then
  -1 stack-depth +! ;

: gen-rshift ( -- )
  \ ( value count -- result ) count=rax, value=rbx
  $51 c,                                \ push rcx
  $48 c, $89 c, $c1 c,                 \ mov rcx, rax
  $48 c, $89 c, $d8 c,                 \ mov rax, rbx
  $48 c, $d3 c, $e8 c,                 \ shr rax, cl
  $59 c,                                \ pop rcx
  stack-depth @ 1- dup
  2 >= if
    $48 c, $89 c, $cb c,               \ mov rbx, rcx
  then
  3 >= if
    $49 c, $8b c, $0f c,               \ mov rcx, [r15]
    $49 c, $83 c, $c7 c, 8 c,          \ add r15, 8
  then
  -1 stack-depth +! ;

\ ============================================================
\ DOUBLE-CELL AND UNSIGNED
\ ============================================================

: gen-s>d ( -- )
  \ ( n -- d ) sign-extend: push rax, then rax = rax >> 63
  push-tos
  $48 c, $89 c, $c3 c,             \ mov rbx, rax  (copy n to NOS)
  $48 c, $c1 c, $f8 c, 63 c, ;    \ sar rax, 63   (TOS = sign: 0 or -1)

: gen-um* ( -- )
  \ ( u1 u2 -- ud ) u2=rax, u1=rbx → rdx:rax = rax*rbx
  \ Result: low in rbx (NOS), high in rax (TOS) — double on stack as ( low high )
  $48 c, $f7 c, $e3 c,             \ mul rbx  → rdx:rax
  $48 c, $89 c, $c3 c,             \ mov rbx, rax  (low → NOS)
  $48 c, $89 c, $d0 c, ;           \ mov rax, rdx  (high → TOS)

: gen-m* ( -- )
  \ ( n1 n2 -- d ) n2=rax, n1=rbx → rdx:rax = rax*rbx (signed)
  $48 c, $f7 c, $eb c,             \ imul rbx → rdx:rax
  $48 c, $89 c, $c3 c,             \ mov rbx, rax  (low → NOS)
  $48 c, $89 c, $d0 c, ;           \ mov rax, rdx  (high → TOS)

: gen-um/mod ( -- )
  \ ( udlo udhi u1 -- ur uq ) u1=rax, udhi=rbx, udlo=3rd
  \ x86 div: rdx:rax / reg → rax=quot, rdx=rem
  \ Strategy: use x86 stack to shuffle registers
  $50 c,                            \ push rax (save divisor)
  $48 c, $89 c, $da c,             \ mov rdx, rbx (udhi → rdx)
  \ Load udlo into rax from 3rd stack position
  stack-depth @ 3 >= if
    $48 c, $89 c, $c8 c,           \ mov rax, rcx (3rd reg → rax = udlo)
  else
    \ udlo on r15 memory stack
    $49 c, $8b c, $07 c,           \ mov rax, [r15]
    $49 c, $83 c, $c7 c, 8 c,     \ add r15, 8
  then
  $59 c,                            \ pop rcx (divisor → rcx)
  $48 c, $f7 c, $f1 c,             \ div rcx → rax=quot, rdx=rem
  \ Result: ( rem quot ) — rem in NOS, quot in TOS
  $48 c, $89 c, $d3 c,             \ mov rbx, rdx (rem → NOS)
  \ rax already has quotient (TOS)
  -1 stack-depth +! ;              \ consumed 3, produced 2

: gen-sm/rem ( -- )
  \ ( dlo dhi n -- rem quot ) signed symmetric division
  \ x86 idiv does symmetric (truncate toward zero) — exactly sm/rem
  $50 c,                            \ push rax (save divisor)
  $48 c, $89 c, $da c,             \ mov rdx, rbx (dhi → rdx)
  stack-depth @ 3 >= if
    $48 c, $89 c, $c8 c,           \ mov rax, rcx (3rd reg → rax = dlo)
  else
    $49 c, $8b c, $07 c,           \ mov rax, [r15]
    $49 c, $83 c, $c7 c, 8 c,     \ add r15, 8
  then
  $59 c,                            \ pop rcx (divisor → rcx)
  $48 c, $f7 c, $f9 c,             \ idiv rcx → rax=quot, rdx=rem
  $48 c, $89 c, $d3 c,             \ mov rbx, rdx (rem → NOS)
  -1 stack-depth +! ;

: gen-fm/mod ( -- )
  \ ( dlo dhi n -- rem quot ) floored division
  \ Floored: if rem != 0 and rem sign differs from divisor, adjust:
  \   rem += divisor, quot -= 1
  \ Strategy: emit idiv, then a conditional fixup
  $50 c,                            \ push rax (save divisor)
  $48 c, $89 c, $da c,             \ mov rdx, rbx (dhi → rdx)
  stack-depth @ 3 >= if
    $48 c, $89 c, $c8 c,           \ mov rax, rcx
  else
    $49 c, $8b c, $07 c,
    $49 c, $83 c, $c7 c, 8 c,
  then
  $59 c,                            \ pop rcx (divisor → rcx)
  $48 c, $f7 c, $f9 c,             \ idiv rcx → rax=quot, rdx=rem
  \ Fixup: if rdx != 0 && (rdx ^ rcx) < 0 then rdx+=rcx, rax-=1
  $48 c, $85 c, $d2 c,             \ test rdx, rdx
  $74 c, 17 c,                     \ jz +17 (skip fixup if rem=0)
  $48 c, $89 c, $d6 c,             \ mov rsi, rdx
  $48 c, $31 c, $ce c,             \ xor rsi, rcx
  $48 c, $85 c, $f6 c,             \ test rsi, rsi
  $7d c, 6 c,                      \ jge +6 (skip if same sign)
  $48 c, $01 c, $ca c,             \ add rdx, rcx
  $48 c, $ff c, $c8 c,             \ dec rax
  \ Result: ( rem quot )
  $48 c, $89 c, $d3 c,             \ mov rbx, rdx (rem → NOS)
  -1 stack-depth +! ;

: gen-d+ ( -- )
  \ ( d1lo d1hi d2lo d2hi -- dlo dhi )
  \ d2hi=rax, d2lo=rbx, d1hi=3rd, d1lo=4th
  \ add with carry: d1lo+d2lo → low, d1hi+d2hi+carry → high
  \ Use x86 stack for register shuffling
  $48 c, $89 c, $c2 c,             \ mov rdx, rax (d2hi → rdx)
  stack-depth @ 3 >= if
    $48 c, $89 c, $c8 c,           \ mov rax, rcx (d1hi → rax)
  else
    $49 c, $8b c, $07 c,           \ mov rax, [r15]
    $49 c, $83 c, $c7 c, 8 c,     \ add r15, 8
  then
  \ Now need d1lo — it's 4th item (on r15 stack regardless)
  $49 c, $8b c, $0f c,             \ mov rcx, [r15] (d1lo)
  $49 c, $83 c, $c7 c, 8 c,       \ add r15, 8
  \ rcx=d1lo, rbx=d2lo, rax=d1hi, rdx=d2hi
  $48 c, $01 c, $d9 c,             \ add rcx, rbx  (low = d1lo + d2lo)
  $48 c, $11 c, $d0 c,             \ adc rax, rdx  (high = d1hi + d2hi + carry)
  $48 c, $89 c, $cb c,             \ mov rbx, rcx  (low → NOS)
  \ rax = high (TOS)
  -2 stack-depth +! ;              \ consumed 4, produced 2

: gen-d- ( -- )
  \ ( d1lo d1hi d2lo d2hi -- dlo dhi )
  $48 c, $89 c, $c2 c,             \ mov rdx, rax (d2hi)
  stack-depth @ 3 >= if
    $48 c, $89 c, $c8 c,           \ mov rax, rcx (d1hi)
  else
    $49 c, $8b c, $07 c,
    $49 c, $83 c, $c7 c, 8 c,
  then
  $49 c, $8b c, $0f c,             \ mov rcx, [r15] (d1lo)
  $49 c, $83 c, $c7 c, 8 c,       \ add r15, 8
  \ rcx=d1lo, rbx=d2lo, rax=d1hi, rdx=d2hi
  $48 c, $29 c, $d9 c,             \ sub rcx, rbx  (low = d1lo - d2lo)
  $48 c, $19 c, $d0 c,             \ sbb rax, rdx  (high = d1hi - d2hi - borrow)
  $48 c, $89 c, $cb c,             \ mov rbx, rcx
  -2 stack-depth +! ;

\ ============================================================
\ MEMORY
\ ============================================================

: gen-@ ( -- )  $48 c, $8b c, $00 c, ;

: gen-! ( -- )
  \ ( value addr -- ) addr=rax, value=rbx
  $48 c, $89 c, $18 c,
  stack-depth @ 3 >= if
    $48 c, $89 c, $c8 c,
    stack-depth @ 4 >= if
      $49 c, $8b c, $0f c,
      $49 c, $83 c, $c7 c, 8 c,
    then
  then
  -2 stack-depth +! ;

: gen-c@ ( -- )  $48 c, $0f c, $b6 c, $00 c, ;

: gen-c! ( -- )
  \ ( char addr -- ) addr=rax, char=rbx
  $88 c, $18 c,
  stack-depth @ 3 >= if
    $48 c, $89 c, $c8 c,
    stack-depth @ 4 >= if
      $49 c, $8b c, $0f c,
      $49 c, $83 c, $c7 c, 8 c,
    then
  then
  -2 stack-depth +! ;

: gen-+! ( -- )
  \ ( n addr -- ) addr=rax, n=rbx
  $48 c, $01 c, $18 c,
  stack-depth @ 3 >= if
    $48 c, $89 c, $c8 c,
    stack-depth @ 4 >= if
      $49 c, $8b c, $0f c,
      $49 c, $83 c, $c7 c, 8 c,
    then
  then
  -2 stack-depth +! ;

: gen-cells ( -- )
  \ ( n -- n*8 ) shl rax, 3
  $48 c, $c1 c, $e0 c, 3 c, ;

: gen-cell+ ( -- )
  \ ( addr -- addr+8 ) add rax, 8
  $48 c, $83 c, $c0 c, 8 c, ;

\ ============================================================
\ RETURN STACK
\ ============================================================

: gen->r ( -- )
  $48 c, $83 c, $ed c, 8 c,
  $48 c, $89 c, $45 c, 0 c,
  stack-depth @ 1 > if pop-tos else -1 stack-depth +! then ;

: gen-r> ( -- )
  push-tos
  $48 c, $8b c, $45 c, 0 c,
  $48 c, $83 c, $c5 c, 8 c, ;

: gen-r@ ( -- )
  push-tos
  $48 c, $8b c, $45 c, 0 c, ;

: gen-2>r ( -- )
  $48 c, $83 c, $ed c, 16 c,
  $48 c, $89 c, $45 c, 8 c,
  stack-depth @ 1 > if pop-tos else -1 stack-depth +! then
  $48 c, $89 c, $45 c, 0 c,
  stack-depth @ 1 > if pop-tos else -1 stack-depth +! then ;

: gen-2r> ( -- )
  push-tos
  $48 c, $8b c, $45 c, 0 c,
  push-tos
  $48 c, $8b c, $45 c, 8 c,
  $48 c, $83 c, $c5 c, 16 c, ;

: gen-2r@ ( -- )
  push-tos
  $48 c, $8b c, $45 c, 0 c,
  push-tos
  $48 c, $8b c, $45 c, 8 c, ;

\ ============================================================
\ CONTROL FLOW
\ ============================================================

: gen-if ( -- orig )
  $48 c, $89 c, $c7 c,
  pop-tos
  $48 c, $85 c, $ff c,
  $0f c, $84 c,
  0 d,
  code-here ;

: gen-<if ( -- orig )
  $49 c, $8b c, $1f c,
  $48 c, $39 c, $c3 c,
  $49 c, $8b c, $47 c, 8 c,
  $4d c, $8d c, $7f c, 16 c,
  $0f c, $8d c,
  0 d,
  code-here ;

: gen->if ( -- orig )
  $49 c, $8b c, $1f c,
  $48 c, $39 c, $c3 c,
  $49 c, $8b c, $47 c, 8 c,
  $4d c, $8d c, $7f c, 16 c,
  $0f c, $8e c,
  0 d,
  code-here ;

: gen-=if ( -- orig )
  $49 c, $8b c, $1f c,
  $48 c, $39 c, $c3 c,
  $49 c, $8b c, $47 c, 8 c,
  $4d c, $8d c, $7f c, 16 c,
  $0f c, $85 c,
  0 d,
  code-here ;

: gen-0<if ( -- orig )
  $48 c, $85 c, $c0 c,
  pop-tos
  $0f c, $89 c,
  0 d,
  code-here ;

: gen-0=if ( -- orig )
  $48 c, $85 c, $c0 c,
  pop-tos
  $0f c, $85 c,
  0 d,
  code-here ;

: gen-else ( orig1 -- orig2 )
  $e9 c,
  0 d,
  code-here
  tuck
  swap 4 -
  patch-rel32 ;

: gen-then ( orig -- )
  code-here swap 4 - patch-rel32 ;

: gen-begin ( -- dest )  code-here ;

: gen-until ( dest -- )
  $48 c, $89 c, $c7 c,
  pop-tos
  $48 c, $85 c, $ff c,
  $0f c, $84 c,
  code-here 4 + - d, ;

: gen-0=until ( dest -- )
  $48 c, $85 c, $c0 c,
  $0f c, $95 c, $c1 c,
  pop-tos
  $84 c, $c9 c,
  $0f c, $85 c,
  code-here 4 + - d, ;

: gen-nzloop ( dest -- )
  $48 c, $85 c, $c0 c,
  $0f c, $85 c,
  code-here 4 + - d, ;

: gen-1-nzloop ( dest -- )
  dup code-here = if
    drop
    $31 c, $c0 c,
    exit
  then
  dup code-here 3 - = if
    code-buf code-here 3 - + c@ $48 = if
    code-buf code-here 2 - + c@ $ff = if
    code-buf code-here 1 - + c@ $c3 = if
      drop
      -3 code-pos +!
      $48 c, $01 c, $c3 c,
      $31 c, $c0 c,
      exit
    then then then
  then
  $48 c, $ff c, $c8 c,
  $0f c, $85 c,
  code-here 4 + - d, ;

\ OPTIMIZATION 8+9: flag elision and loop fusion (see header)
\ DO NOT REMOVE or reorder any of: last-sets-flags?, gen-while-fused,
\ gen-until-fused, gen-repeat. They form one interlocking optimization.
\ last-sets-flags? lets gen-while-fused skip redundant test rax,rax.
\ gen-repeat fuses while+repeat and eliminates countdown loops.
: last-sets-flags? ( -- flag )
  code-here 3 < if 0 exit then
  code-buf code-here 3 - + c@ $48 <> if 0 exit then
  code-buf code-here 2 - + c@ $ff <> if 0 exit then
  code-buf code-here 1 - + c@ dup $c8 = swap $c0 = or ;

\ Fused while: conditional forward jump (skip test if dec/inc already set flags)
\ cmp=1(0=): exit if NOT zero → jnz
\ cmp=2(0>): exit if NOT >0   → jle
\ cmp=3(0<): exit if NOT <0   → jge
: gen-while-fused ( dest cmp -- orig dest )
  last-sets-flags? 0= if $48 c, $85 c, $c0 c, then  \ test rax,rax (skip if flags set)
  dup 1 = if drop $0f c, $85 c, else  \ jnz (exit when not-zero, i.e. 0= failed)
  dup 2 = if drop $0f c, $8e c, else  \ jle (exit when <=0, i.e. 0> failed)
      3 = if   $0f c, $89 c,          \ jns/jge (exit when >=0, i.e. 0< failed)
  then then then
  0 d,
  code-here swap ;

\ Fused until: conditional backward jump when condition is FALSE (skip test if flags set)
\ (until loops when false, exits when true)
\ cmp=1(0=): loop while not-zero → jnz
\ cmp=2(0>): loop while <=0      → jle
\ cmp=3(0<): loop while >=0      → jge
: gen-until-fused ( dest cmp -- )
  last-sets-flags? 0= if $48 c, $85 c, $c0 c, then  \ test rax,rax (skip if flags set)
  dup 1 = if drop $0f c, $85 c, else  \ jnz
  dup 2 = if drop $0f c, $8e c, else  \ jle
      3 = if   $0f c, $8d c,          \ jge
  then then then
  code-here 4 + - d, ;

: gen-while ( dest -- orig dest )
  $48 c, $89 c, $c7 c,
  pop-tos
  $48 c, $85 c, $ff c,
  $0f c, $84 c,
  0 d,
  code-here swap ;

\ OPTIMIZATION 9: while/repeat fusion + loop elimination (see header)
\ DO NOT CHANGE byte-read pattern. dup copies dest (TOS), NOT orig.
\ (code-buf dup +) is WRONG — gives code-buf+code-buf. (dup code-buf +) is RIGHT.
: gen-repeat ( orig dest -- )
  over code-here = if
    \ Adjacent while/repeat: check for loop elimination
    \ Body is from dest to conditional jump (6 bytes before orig)
    over 6 - over -   ( orig dest body-size )
    dup 3 = if drop
      \ Check if body is dec rax (48 ff c8) = pure countdown
      dup code-buf + >r
      r@ c@ $48 =  r@ 1+ c@ $ff = and  r@ 2 + c@ $c8 = and
      r> drop if
        nip code-pos !  $31 c, $c0 c,  exit  \ xor eax,eax
      then
    else dup 6 = if drop
      \ Check if body is inc rbx; dec rax (nos++ countdown)
      dup code-buf + >r
      r@ c@ $48 =  r@ 1+ c@ $ff = and  r@ 2 + c@ $c3 = and
      r@ 3 + c@ $48 = and  r@ 4 + c@ $ff = and  r@ 5 + c@ $c8 = and
      r> drop if
        nip code-pos !
        $48 c, $01 c, $c3 c,  $31 c, $c0 c,  exit  \ add rbx,rax; xor eax,eax
      then
    else drop then then
    \ Not eliminated: fuse into backward conditional jump
    \ Invert condition byte at orig-5 (0f XX → 0f XX^1)
    over 5 - code-buf + dup c@ 1 xor swap c!
    \ Patch displacement to jump backward to dest
    swap 4 - patch-rel32
    exit
  then
  $e9 c,
  code-here 4 + - d,
  code-here swap 4 - patch-rel32 ;

: gen-again ( dest -- )
  $e9 c,
  code-here 4 + - d, ;

\ ============================================================
\ CALLS
\ ============================================================

variable call-nargs  1 call-nargs !
variable call-rets   1 call-rets !
: gen-call ( addr -- )
  call-nargs @ 0= call-rets @ 0> and if push-tos 1 call-nargs ! then
  stack-depth @ 3 >= call-nargs @ 3 < and if $51 c, then
  stack-depth @ 2 >= call-nargs @ 2 < and if $53 c, then
  $e8 c,
  code-here 4 + - d,
  stack-depth @ 2 >= call-nargs @ 2 < and if
    call-rets @ 2 < if $5b c, else $59 c, then   \ pop rbx or pop rcx based on rets
  then
  stack-depth @ 3 >= call-nargs @ 3 < and call-rets @ 3 < and if $59 c, then
  call-nargs @ 2 = call-rets @ 1 = and if
    stack-depth @ 3 >= if
      $48 c, $89 c, $cb c,                  \ mov rbx, rcx
      stack-depth @ 4 >= if
        $49 c, $8b c, $0f c,                \ mov rcx, [r15]
        $49 c, $83 c, $c7 c, 8 c,           \ add r15, 8
      then
    then
    -1 stack-depth +!
  else
    call-rets @ call-nargs @ - stack-depth +!
  then ;

: gen-ret ( -- )
  do-depth @ 0 ?do
    $41 c, $5d c,
    $41 c, $5c c,
  loop
  $c3 c, ;

\ ============================================================
\ DO/LOOP
\ ============================================================

: gen-do ( -- do-addr )
  1 do-depth +!
  0 do-depth @ 1- cells do-leave + !  \ clear leave patch for this level
  $41 c, $54 c,
  $41 c, $55 c,
  $49 c, $89 c, $dd c,
  $49 c, $89 c, $c4 c,
  stack-depth @ 2 -
  dup 2 = if
    $48 c, $89 c, $c8 c,
    $49 c, $8b c, $1f c,
    $49 c, $83 c, $c7 c, 8 c,
  else dup 2 > if
    $48 c, $89 c, $c8 c,
    $49 c, $8b c, $1f c,
    $49 c, $8b c, $4f c, 8 c,
    $49 c, $83 c, $c7 c, 16 c,
  else dup 1 = if
    $48 c, $89 c, $c8 c,
  then then then
  stack-depth !
  $4d c, $39 c, $ec c,
  $0f c, $84 c,
  0 d,
  code-here cf-push
  code-here cf-push ;

: gen-?do ( -- do-addr )
  \ Like gen-do but skips loop if limit <= start (uses jge instead of jz)
  1 do-depth +!
  0 do-depth @ 1- cells do-leave + !
  $41 c, $54 c,                       \ push r12
  $41 c, $55 c,                       \ push r13
  $49 c, $89 c, $dd c,                \ mov r13, rbx (limit)
  $49 c, $89 c, $c4 c,                \ mov r12, rax (start/index)
  stack-depth @ 2 -
  dup 2 = if
    $48 c, $89 c, $c8 c,              \ mov rax, rcx
    $49 c, $8b c, $1f c,              \ mov rbx, [r15]
    $49 c, $83 c, $c7 c, 8 c,         \ add r15, 8
  else dup 2 > if
    $48 c, $89 c, $c8 c,              \ mov rax, rcx
    $49 c, $8b c, $1f c,              \ mov rbx, [r15]
    $49 c, $8b c, $4f c, 8 c,         \ mov rcx, [r15+8]
    $49 c, $83 c, $c7 c, 16 c,        \ add r15, 16
  else dup 1 = if
    $48 c, $89 c, $c8 c,              \ mov rax, rcx
  then then then
  stack-depth !
  $4d c, $39 c, $ec c,                \ cmp r12, r13
  $0f c, $8d c,                       \ jge rel32 (skip if index >= limit)
  0 d,
  code-here cf-push
  code-here cf-push ;

: gen-loop ( -- )
  cf-pop  ( loop-start )
  \ OPTIMIZATION 10: do/loop elimination for constant-trip 1+ loops.
  \ If body is inc rax (48 ff c0) and trip count is known, replace with add rax, N.
  do-depth @ 1- cells do-trip + @ dup -1 <> over 0> and if  ( loop-start trip )
    over code-here swap - 3 = if      ( loop-start trip )
      over code-buf + >r
      r@ c@ $48 =  r@ 1+ c@ $ff = and  r@ 2 + c@ $c0 = and
      r> drop if
        nip  ( trip )
        do-depth @ 1- cells do-origin + @ code-pos !
        do-depth @ 1- cells do-sdepth + @ stack-depth !
        cf-pop drop
        gen-add-imm
        -1 do-depth +! exit
      then
    then drop               ( loop-start )
  else drop then            ( loop-start )
  \ Normal loop: inc r12, cmp, jl back
  $49 c, $ff c, $c4 c,
  $4d c, $39 c, $ec c,
  $0f c, $8c c,
  code-here 4 + - d,
  cf-pop
  code-here swap 4 - patch-rel32
  $41 c, $5d c,
  $41 c, $5c c,
  \ Patch any pending leave for this loop level (after pops)
  do-depth @ 1- cells do-leave + @ ?dup if
    code-here swap 4 - patch-rel32
  then
  -1 do-depth +! ;

: gen-+loop ( -- )
  cf-pop
  $48 c, $89 c, $c7 c,
  $49 c, $01 c, $c4 c,
  pop-tos
  $48 c, $85 c, $ff c,
  $78 c, 11 c,
  $4d c, $39 c, $ec c,
  $0f c, $8c c,
  dup code-here 4 + - d,
  $eb c, 9 c,
  $4d c, $39 c, $ec c,
  $0f c, $8d c,
  dup code-here 4 + - d,
  drop
  cf-pop
  code-here swap 4 - patch-rel32
  $41 c, $5d c,
  $41 c, $5c c,
  \ Patch any pending leave for this loop level (after pops)
  do-depth @ 1- cells do-leave + @ ?dup if
    code-here swap 4 - patch-rel32
  then
  -1 do-depth +! ;

: gen-i ( -- )
  push-tos
  $4c c, $89 c, $e0 c, ;

: gen-j ( -- )
  push-tos
  $48 c, $8b c, $44 c, $24 c, 8 c, ;

: gen-leave ( -- )
  \ Emit: pop r13, pop r12, jmp forward (to after loop)
  $41 c, $5d c,                  \ pop r13
  $41 c, $5c c,                  \ pop r12
  $e9 c, 0 d,                    \ jmp rel32 (placeholder)
  code-here do-depth @ 1- cells do-leave + ! ;  \ save patch addr

: gen-unloop ( -- )
  \ Remove DO..LOOP params from return stack (for early EXIT)
  $41 c, $5d c,                  \ pop r13
  $41 c, $5c c, ;                \ pop r12

\ ============================================================
\ RECURSE
\ ============================================================
\ OPTIMIZATION 5: TAIL-CALL OPTIMIZATION.
\ When recurse is the last word before ;, end-def patches the
\ call instruction (e8) to a jmp (e9), eliminating stack frame overhead.
\ tail-recurse records the code address of the last recurse call.

variable current-word-addr  0 current-word-addr !

: gen-recurse ( -- )
  arg-count @ call-nargs !
  current-word-addr @ gen-call ;

: gen-tail-recurse ( -- )
  $e9 c,
  current-word-addr @ code-here 4 + - d, ;

variable tail-recurse  0 tail-recurse !
variable is-recursive  0 is-recursive !

\ ============================================================
\ PROLOGUE / EPILOGUE
\ ============================================================

variable start-jmp

: gen-prologue ( -- )
  \ Save argc and argv from Linux stack before setting up our stacks
  $48 c, $8b c, $04 c, $24 c,              \ mov rax, [rsp]  (argc)
  $48 c, $89 c, $04 c, $25 c, rt-argc d,   \ mov [rt-argc], rax
  $48 c, $8d c, $44 c, $24 c, 8 c,         \ lea rax, [rsp+8]  (argv array)
  $48 c, $89 c, $04 c, $25 c, rt-argv d,   \ mov [rt-argv], rax
  \ Set up data and return stacks
  $49 c, $bf c, $800000 $100000 + q,       \ r15 = data stack at 0x900000
  $48 c, $bd c, $800000 $180000 + q,       \ rbp = return stack at 0x980000
  $e9 c, code-here start-jmp ! 0 d, ;      \ jmp to main code

: patch-start ( -- )
  code-here start-jmp @ patch-rel32 ;

: gen-epilogue ( -- )
  $b8 c, 60 d,
  $31 c, $ff c,
  $0f c, $05 c, ;

\ ============================================================
\ PRINT NUMBER (for . word)
\ ============================================================

: gen-dot ( -- )
  1 has-io !
  stack-depth @ 3 >= if $51 c, then
  $45 c, $31 c, $c0 c,
  $48 c, $85 c, $c0 c,
  $79 c, 28 c,
  $50 c,
  $b8 c, 1 d,
  $bf c, 1 d,
  $6a c, 45 c,
  $48 c, $89 c, $e6 c,
  $ba c, 1 d,
  $0f c, $05 c,
  $58 c,
  $58 c,
  $48 c, $f7 c, $d8 c,
  code-here
  $48 c, $c7 c, $c1 c, 10 d,
  $48 c, $31 c, $d2 c,
  $48 c, $f7 c, $f1 c,
  $83 c, $c2 c, $30 c,
  $52 c,
  $41 c, $ff c, $c0 c,
  $48 c, $85 c, $c0 c,
  $75 c,
  dup code-here 1+ - c,
  drop
  code-here
  $b8 c, 1 d,
  $bf c, 1 d,
  $48 c, $89 c, $e6 c,
  $ba c, 1 d,
  $0f c, $05 c,
  $58 c,
  $41 c, $ff c, $c8 c,
  $75 c,
  dup code-here 1+ - c,
  drop
  $6a c, 32 c,
  $b8 c, 1 d,
  $bf c, 1 d,
  $48 c, $89 c, $e6 c,
  $ba c, 1 d,
  $0f c, $05 c,
  $58 c,
  stack-depth @ 3 >= if $59 c, then
  pop-tos ;

: gen-u. ( -- )
  \ ( u -- ) unsigned print with trailing space
  \ Same as gen-dot but no sign handling
  1 has-io !
  stack-depth @ 3 >= if $51 c, then       \ save rcx
  $45 c, $31 c, $c0 c,                    \ xor r8d, r8d (digit count)
  \ Division loop: div rax by 10, push remainder as digit
  code-here
  $48 c, $c7 c, $c1 c, 10 d,             \ mov rcx, 10
  $48 c, $31 c, $d2 c,                    \ xor rdx, rdx
  $48 c, $f7 c, $f1 c,                    \ div rcx → rax=quot, rdx=rem
  $83 c, $c2 c, $30 c,                    \ add edx, '0'
  $52 c,                                   \ push rdx (digit)
  $41 c, $ff c, $c0 c,                    \ inc r8d (digit count)
  $48 c, $85 c, $c0 c,                    \ test rax, rax
  $75 c,                                   \ jnz back to div loop
  dup code-here 1+ - c,
  drop
  \ Print loop: pop and write each digit
  code-here
  $b8 c, 1 d,                             \ mov eax, 1
  $bf c, 1 d,                             \ mov edi, 1
  $48 c, $89 c, $e6 c,                    \ mov rsi, rsp
  $ba c, 1 d,                             \ mov edx, 1
  $0f c, $05 c,                            \ syscall
  $58 c,                                   \ pop (discard digit)
  $41 c, $ff c, $c8 c,                    \ dec r8d
  $75 c,                                   \ jnz back to print loop
  dup code-here 1+ - c,
  drop
  \ Trailing space
  $6a c, 32 c,                            \ push 32
  $b8 c, 1 d,                             \ mov eax, 1
  $bf c, 1 d,                             \ mov edi, 1
  $48 c, $89 c, $e6 c,                    \ mov rsi, rsp
  $ba c, 1 d,                             \ mov edx, 1
  $0f c, $05 c,                            \ syscall
  $58 c,                                   \ pop scratch
  stack-depth @ 3 >= if $59 c, then       \ restore rcx
  pop-tos ;

: gen-cr ( -- )
  1 has-io !
  $50 c,
  stack-depth @ 2 >= if $53 c, then
  stack-depth @ 3 >= if $51 c, then
  $6a c, 10 c,
  $b8 c, 1 d,
  $bf c, 1 d,
  $48 c, $89 c, $e6 c,
  $ba c, 1 d,
  $0f c, $05 c,
  $58 c,
  stack-depth @ 3 >= if $59 c, then
  stack-depth @ 2 >= if $5b c, then
  $58 c, ;

: gen-emit ( -- )
  1 has-io !
  stack-depth @ 2 >= if $53 c, then
  stack-depth @ 3 >= if $51 c, then
  $50 c,
  $b8 c, 1 d,
  $bf c, 1 d,
  $48 c, $89 c, $e6 c,
  $ba c, 1 d,
  $0f c, $05 c,
  $58 c,
  stack-depth @ 3 >= if $59 c, then
  stack-depth @ 2 >= if $5b c, then
  pop-tos ;

: gen-type ( -- )
  \ ( addr u -- ) u=rax, addr=rbx
  1 has-io !
  stack-depth @ 3 >= if $51 c, then
  $48 c, $89 c, $c2 c,
  $48 c, $89 c, $de c,
  $b8 c, 1 d,
  $bf c, 1 d,
  $0f c, $05 c,
  stack-depth @ 3 >= if $59 c, then
  -2 stack-depth +!
  stack-depth @ 1 >= if
    $48 c, $89 c, $c8 c,
    stack-depth @ 2 >= if
      $49 c, $8b c, $0f c,
      $49 c, $83 c, $c7 c, 8 c,
    then
  then ;

: gen-s" ( -- )
  \ Parse string, emit inline: jmp over; data; over: push addr, push len
  1 input-pos +!
  $e9 c, code-here >r 0 d,       \ emit jmp, save patch-addr on rstack
  code-here 0                     \ ( str-start len )
  begin
    input-pos @ input-len @ >= if true else
    input-buf input-pos @ + c@ [char] " = if
      1 input-pos +! true
    else
      input-buf input-pos @ + c@ c, 1+
      1 input-pos +! false
    then then
  until
  code-here r> patch-rel32        \ patch jmp to skip over string data
  swap $4000B0 + swap             \ ( runtime-addr len )
  push-tos
  $48 c, $b8 c, swap q,          \ mov rax, addr
  push-tos
  $48 c, $b8 c, q, ;             \ mov rax, len

: gen-dotquote ( -- )
  gen-s"
  gen-type ;

\ ============================================================
\ FILE I/O
\ ============================================================
\ Runtime file operations use Linux syscalls directly.
\ OPEN-FILE needs null-terminated path, so we copy to rt-word-buf.

: gen-r/o ( -- )  push-tos  $31 c, $c0 c, ;  \ 0 = O_RDONLY
: gen-w/o ( -- )  push-tos  $b8 c, 577 d, ;  \ O_WRONLY|O_CREAT|O_TRUNC = 1|64|512
: gen-r/w ( -- )  push-tos  $b8 c, 2 d, ;    \ 2 = O_RDWR

: gen-argc ( -- )  \ ( -- n )
  push-tos
  $48 c, $8b c, $04 c, $25 c, rt-argc d,   \ mov rax, [rt-argc]
  1 stack-depth +! ;

: gen-argv ( -- )  \ ( n -- addr u )
  \ Get argv[n] as (addr u) string pair
  \ Input: rax = n, stack-depth = 1
  $48 c, $8b c, $3c c, $25 c, rt-argv d,   \ mov rdi, [rt-argv]
  $48 c, $8b c, $3c c, $c7 c,              \ mov rdi, [rdi+rax*8] = argv[n]
  \ rdi = pointer to null-terminated string, find length
  $48 c, $31 c, $c0 c,                     \ xor rax, rax (length counter)
  code-here                                 \ loop:
  $80 c, $3c c, $07 c, 0 c,                \ cmp byte [rdi+rax], 0
  $74 c, 5 c,                              \ jz done
  $48 c, $ff c, $c0 c,                     \ inc rax
  $eb c, code-here - 1- c,                  \ jmp loop
  \ rax = length, rdi = start address
  \ Result: NOS=addr, TOS=len
  $48 c, $89 c, $fb c,                     \ mov rbx, rdi (addr)
  \ rax already has length
  1 stack-depth +! ;                        \ depth 1->2

: gen-open-file ( -- )
  \ ( addr u fam -- fileid ior )
  \ fam=rax, u=rbx, addr=rcx or [r15]
  \ Uses rt-word-buf as temp buffer for null-terminated path
  1 has-io !
  \ Save registers
  stack-depth @ 3 >= if
    $48 c, $89 c, $ce c,                    \ mov rsi, rcx (addr from 3rd reg)
  else
    $49 c, $8b c, $37 c,                    \ mov rsi, [r15] (addr from memory)
    $49 c, $83 c, $c7 c, 8 c,              \ add r15, 8
  then
  \ rsi=addr, rbx=len, rax=fam
  \ Copy string to rt-word-buf and null-terminate
  $50 c,                                    \ push rax (save fam)
  $48 c, $bf c, rt-word-buf q,             \ mov rdi, rt-word-buf
  $48 c, $89 c, $d9 c,                     \ mov rcx, rbx (len)
  $f3 c, $a4 c,                            \ rep movsb (copy string)
  $c6 c, $07 c, 0 c,                       \ mov byte [rdi], 0 (null-terminate)
  \ Now call open syscall
  $58 c,                                    \ pop rax (fam -> flags)
  $48 c, $89 c, $c6 c,                     \ mov rsi, rax (flags)
  $48 c, $bf c, rt-word-buf q,             \ mov rdi, rt-word-buf (path)
  $ba c, 420 d,                            \ mov edx, 0644 (mode)
  $b8 c, 2 d,                              \ mov eax, 2 (sys_open)
  $0f c, $05 c,                            \ syscall
  \ rax = fd (>=0) or -errno (<0)
  \ Convert to fileid + ior
  $48 c, $89 c, $c3 c,                     \ mov rbx, rax (save fd)
  $48 c, $31 c, $c0 c,                     \ xor eax, eax (ior = 0 = success)
  $48 c, $85 c, $db c,                     \ test rbx, rbx
  $79 c, 6 c,                              \ jns +6 (skip if fd >= 0)
  $48 c, $31 c, $db c,                     \ xor rbx, rbx (fileid = 0 on error)
  $48 c, $ff c, $c8 c,                     \ dec rax (ior = -1 on error)
  \ Stack result: rbx=fileid, rax=ior
  \ Consumed 3 (addr u fam), produced 2 (fileid ior) = net -1
  -1 stack-depth +!
  ;

: gen-create-file ( -- )
  \ ( addr u fam -- fileid ior )
  \ Like open-file but with O_CREAT | O_TRUNC added to flags
  1 has-io !
  stack-depth @ 3 >= if
    $48 c, $89 c, $ce c,                    \ mov rsi, rcx (addr from 3rd reg)
  else
    $49 c, $8b c, $37 c,                    \ mov rsi, [r15] (addr from memory)
    $49 c, $83 c, $c7 c, 8 c,              \ add r15, 8
  then
  \ rsi=addr, rbx=len, rax=fam
  $50 c,                                    \ push rax (save fam)
  $48 c, $bf c, rt-word-buf q,             \ mov rdi, rt-word-buf
  $48 c, $89 c, $d9 c,                     \ mov rcx, rbx (len)
  $f3 c, $a4 c,                            \ rep movsb
  $c6 c, $07 c, 0 c,                       \ mov byte [rdi], 0
  $58 c,                                    \ pop rax (fam)
  $48 c, $0d c, 576 d,                     \ or rax, O_CREAT|O_TRUNC (64|512)
  $48 c, $89 c, $c6 c,                     \ mov rsi, rax (flags)
  $48 c, $bf c, rt-word-buf q,             \ mov rdi, rt-word-buf
  $ba c, 420 d,                            \ mov edx, 0644
  $b8 c, 2 d,                              \ mov eax, 2 (sys_open)
  $0f c, $05 c,                            \ syscall
  $48 c, $89 c, $c3 c,                     \ mov rbx, rax
  $48 c, $31 c, $c0 c,                     \ xor eax, eax
  $48 c, $85 c, $db c,                     \ test rbx, rbx
  $79 c, 6 c,                              \ jns +6
  $48 c, $31 c, $db c,                     \ xor rbx, rbx
  $48 c, $ff c, $c8 c,                     \ dec rax
  -1 stack-depth +!
  ;

: gen-close-file ( -- )
  \ ( fileid -- ior )
  \ fileid=rax
  1 has-io !
  $48 c, $89 c, $c7 c,                     \ mov rdi, rax (fd)
  $b8 c, 3 d,                              \ mov eax, 3 (sys_close)
  $0f c, $05 c,                            \ syscall
  \ rax = 0 on success, -errno on failure
  $48 c, $85 c, $c0 c,                     \ test rax, rax
  $74 c, 7 c,                              \ jz +7 (skip if success)
  $48 c, $c7 c, $c0 c, -1 d,              \ mov rax, -1
  ;

: gen-read-file ( -- )
  \ ( addr u fileid -- u2 ior )
  \ fileid=rax, u=rbx, addr=rcx or [r15]
  1 has-io !
  stack-depth @ 3 >= if
    $48 c, $89 c, $ce c,                    \ mov rsi, rcx (addr from 3rd)
  else
    $49 c, $8b c, $37 c,                    \ mov rsi, [r15]
    $49 c, $83 c, $c7 c, 8 c,              \ add r15, 8
  then
  \ rsi=addr, rbx=u, rax=fileid
  $48 c, $89 c, $c7 c,                     \ mov rdi, rax (fd)
  $48 c, $89 c, $da c,                     \ mov rdx, rbx (count)
  $48 c, $31 c, $c0 c,                     \ xor eax, eax (sys_read=0)
  $0f c, $05 c,                            \ syscall
  \ rax = bytes read (>=0) or -errno (<0)
  $48 c, $89 c, $c3 c,                     \ mov rbx, rax (save count)
  $48 c, $31 c, $c0 c,                     \ xor eax, eax (ior = 0)
  $48 c, $85 c, $db c,                     \ test rbx, rbx
  $79 c, 6 c,                              \ jns +6
  $48 c, $31 c, $db c,                     \ xor rbx, rbx (u2 = 0 on error)
  $48 c, $ff c, $c8 c,                     \ dec rax (ior = -1)
  \ Consumed 3 (addr u fileid), produced 2 (u2 ior) = net -1
  -1 stack-depth +!
  ;

: gen-write-file ( -- )
  \ ( addr u fileid -- ior )
  \ fileid=rax, u=rbx, addr=rcx or [r15]
  1 has-io !
  stack-depth @ 3 >= if
    $48 c, $89 c, $ce c,                    \ mov rsi, rcx (addr)
  else
    $49 c, $8b c, $37 c,                    \ mov rsi, [r15]
    $49 c, $83 c, $c7 c, 8 c,              \ add r15, 8
  then
  $48 c, $89 c, $c7 c,                     \ mov rdi, rax (fd)
  $48 c, $89 c, $da c,                     \ mov rdx, rbx (count)
  $b8 c, 1 d,                              \ mov eax, 1 (sys_write)
  $0f c, $05 c,                            \ syscall
  \ rax = bytes written (>=0) or -errno (<0)
  \ Return 0 for success, -1 for error
  $48 c, $85 c, $c0 c,                     \ test rax, rax
  $78 c, 5 c,                              \ js +5 (jump if error)
  $48 c, $31 c, $c0 c,                     \ xor eax, eax (success = 0)
  $eb c, 7 c,                              \ jmp +7 (skip error path)
  $48 c, $c7 c, $c0 c, -1 d,              \ mov rax, -1 (error)
  -2 stack-depth +!
  ;

: gen-slurp-file ( -- )
  \ ( addr u -- addr2 u2 )
  \ Reads file into rt-slurp-buf. Returns 0 0 on error.
  1 has-io !
  \ Copy path to rt-word-buf and null-terminate
  $48 c, $bf c, rt-word-buf q,             \ mov rdi, rt-word-buf
  $48 c, $89 c, $de c,                     \ mov rsi, rbx
  $48 c, $89 c, $c1 c,                     \ mov rcx, rax
  $f3 c, $a4 c,                            \ rep movsb
  $c6 c, $07 c, 0 c,                       \ mov byte [rdi], 0
  \ Open syscall
  $48 c, $bf c, rt-word-buf q,             \ mov rdi, rt-word-buf
  $31 c, $f6 c,                            \ xor esi, esi
  $31 c, $d2 c,                            \ xor edx, edx
  $b8 c, 2 d,                              \ mov eax, 2
  $0f c, $05 c,                            \ syscall
  $48 c, $85 c, $c0 c,                     \ test rax, rax
  $78 c, 46 c,                             \ js +46 to error
  $57 c,                                    \ push rdi (will reuse for close)
  $48 c, $89 c, $c7 c,                     \ mov rdi, rax
  $57 c,                                    \ push rdi (fd)
  \ Read syscall
  $48 c, $be c, rt-slurp-buf q,            \ mov rsi, rt-slurp-buf
  $ba c, SLURP-SIZE d,                     \ mov edx, SLURP-SIZE
  $31 c, $c0 c,                            \ xor eax, eax
  $0f c, $05 c,                            \ syscall
  $50 c,                                    \ push rax (bytes)
  \ Close syscall
  $48 c, $8b c, $7c c, $24 c, 8 c,         \ mov rdi, [rsp+8]
  $b8 c, 3 d,                              \ mov eax, 3
  $0f c, $05 c,                            \ syscall
  \ Return
  $58 c,                                    \ pop rax (bytes)
  $48 c, $83 c, $c4 c, 16 c,               \ add rsp, 16
  $48 c, $bb c, rt-slurp-buf q,            \ mov rbx, rt-slurp-buf
  $eb c, 4 c,                              \ jmp +4
  \ Error
  $31 c, $c0 c,                            \ xor eax, eax
  $31 c, $db c,                            \ xor ebx, ebx
  ;

\ ============================================================
\ PICTURED NUMERIC OUTPUT
\ ============================================================
\ Runtime layout: rt-base(8), rt-pno-pos(8), rt-pno-buf(128) at DATA-BASE

: gen-base ( -- )
  \ Push address of base variable
  push-tos
  $48 c, $b8 c, rt-base q, ;            \ mov rax, rt-base

: gen-<# ( -- )
  \ Store 128 into pno-pos
  $48 c, $c7 c, $04 c, $25 c,            \ mov qword [rt-pno-pos], 128
  rt-pno-pos d, PNO-SIZE d, ;

: gen-hold ( -- )
  \ ( char -- ) decrement pno-pos, store char. Uses rdi (free reg).
  $48 c, $8b c, $3c c, $25 c, rt-pno-pos d,  \ mov rdi, [rt-pno-pos]
  $48 c, $ff c, $cf c,                        \ dec rdi
  $48 c, $89 c, $3c c, $25 c, rt-pno-pos d,  \ mov [rt-pno-pos], rdi
  $48 c, $81 c, $c7 c, rt-pno-buf d,         \ add rdi, rt-pno-buf (imm32)
  $40 c, $88 c, $07 c,                        \ mov [rdi], al
  pop-tos ;

: gen-sign ( -- )
  \ ( n -- ) if n < 0, hold '-'. Uses rdi (free reg).
  $48 c, $85 c, $c0 c,                  \ test rax, rax
  $79 c, 0 c, code-here >r              \ jns skip (patch later)
  \ negative: decrement pno-pos, store '-'
  $48 c, $8b c, $3c c, $25 c, rt-pno-pos d,  \ mov rdi, [rt-pno-pos]
  $48 c, $ff c, $cf c,                        \ dec rdi
  $48 c, $89 c, $3c c, $25 c, rt-pno-pos d,  \ mov [rt-pno-pos], rdi
  $48 c, $81 c, $c7 c, rt-pno-buf d,         \ add rdi, rt-pno-buf (imm32)
  $c6 c, $07 c, 45 c,                         \ mov byte [rdi], '-'
  code-here r@ - r> 1- code-buf + c!           \ patch jns offset (relative)
  pop-tos ;

: gen-# ( -- )
  \ ( ud-lo ud-hi -- ud-lo' ud-hi' ) double-cell divide by base, hold remainder
  \ rax=d-hi(TOS), rbx=d-lo(NOS). Uses rsi for base, rdi for buffer. No rcx touch.
  $48 c, $31 c, $d2 c,                        \ xor rdx, rdx
  $48 c, $8b c, $34 c, $25 c, rt-base d,     \ mov rsi, [rt-base]
  $48 c, $f7 c, $f6 c,                        \ div rsi (0:d-hi/base → rax=hi-quot, rdx=hi-rem)
  $48 c, $87 c, $c3 c,                        \ xchg rax, rbx (rax=d-lo, rbx=hi-quot)
  $48 c, $f7 c, $f6 c,                        \ div rsi (hi-rem:d-lo/base → rax=lo-quot, rdx=rem)
  $48 c, $87 c, $c3 c,                        \ xchg rax, rbx (rax=hi-quot=TOS, rbx=lo-quot=NOS)
  \ Convert remainder to ASCII and hold
  $80 c, $c2 c, 48 c,                         \ add dl, '0'
  $80 c, $fa c, 58 c,                         \ cmp dl, '9'+1
  $72 c, 3 c,                                 \ jb skip
  $80 c, $c2 c, 7 c,                          \ add dl, 'A'-'9'-1 (=7) for hex
  \ Decrement pno-pos, store digit using rdi
  $48 c, $8b c, $3c c, $25 c, rt-pno-pos d,  \ mov rdi, [rt-pno-pos]
  $48 c, $ff c, $cf c,                        \ dec rdi
  $48 c, $89 c, $3c c, $25 c, rt-pno-pos d,  \ mov [rt-pno-pos], rdi
  $48 c, $81 c, $c7 c, rt-pno-buf d,         \ add rdi, rt-pno-buf (imm32)
  $88 c, $17 c, ;                              \ mov [rdi], dl

: gen-#s ( -- )
  \ ( ud-lo ud-hi -- 0 0 ) loop: # until both cells are 0
  code-here                                    \ loop target
  gen-#
  $48 c, $89 c, $c7 c,                        \ mov rdi, rax (save hi)
  $48 c, $09 c, $d8 c,                        \ or rax, rbx (test both cells)
  $48 c, $89 c, $f8 c,                        \ mov rax, rdi (restore hi)
  $75 c,                                       \ jnz back
  dup code-here 1+ - c, drop ;                \ patch relative offset

: gen-#> ( -- )
  \ ( ud-lo ud-hi -- addr u ) replace double with string addr and len
  \ rax=hi(TOS→len), rbx=lo(NOS→addr)
  $48 c, $bb c, rt-pno-buf q,                 \ mov rbx, rt-pno-buf
  $48 c, $03 c, $1c c, $25 c, rt-pno-pos d,  \ add rbx, [rt-pno-pos] (NOS=addr)
  $48 c, $c7 c, $c0 c, PNO-SIZE d,            \ mov rax, 128
  $48 c, $2b c, $04 c, $25 c, rt-pno-pos d, ; \ sub rax, [rt-pno-pos] (TOS=len)

\ ============================================================
\ INPUT PARSING
\ ============================================================
\ Runtime: source-buf(4096), >in(8), source-len(8), word-buf(64) at DATA-BASE+144

: gen-source ( -- )
  \ ( -- addr u ) push input buffer address and length
  push-tos
  full-source @ if
    $48 c, $8b c, $04 c, $25 c, rt-source-addr d,  \ mov rax, [rt-source-addr] (indirect)
  else
    $48 c, $b8 c, rt-source-buf q,                 \ mov rax, rt-source-buf (direct)
  then
  push-tos
  $48 c, $8b c, $04 c, $25 c, rt-source-len d, ; \ mov rax, [rt-source-len] (len)

: gen->in ( -- )
  \ ( -- addr ) push address of >in variable
  push-tos
  $48 c, $b8 c, rt->in q, ;                   \ mov rax, rt->in

: gen-parse ( -- )
  \ ( char -- addr u ) parse from >in to delimiter
  \ rax = delimiter char. Result: rbx=addr, rax=len
  \ Uses: rsi=source-buf, rdi=>in, rdx=source-len, r8=start
  $48 c, $89 c, $c7 c,                        \ mov rdi, rax (save delim in rdi)
  $48 c, $8b c, $34 c, $25 c, rt->in d,       \ mov rsi, [rt->in]
  $48 c, $8b c, $14 c, $25 c, rt-source-len d, \ mov rdx, [rt-source-len]
  $49 c, $89 c, $f0 c,                        \ mov r8, rsi (start = >in)
  \ Loop: find delimiter or end
  code-here                                    \ loop target
  $48 c, $39 c, $d6 c,                        \ cmp rsi, rdx
  $7d c, 0 c, code-here >r                    \ jge done (patch later)
  $48 c, $b8 c, rt-source-buf q,              \ mov rax, rt-source-buf
  $0f c, $b6 c, $04 c, $30 c,                 \ movzx eax, byte [rax+rsi]
  $48 c, $39 c, $f8 c,                        \ cmp rax, rdi (delim?)
  $74 c, 0 c, code-here >r                    \ je found (patch later)
  $48 c, $ff c, $c6 c,                        \ inc rsi
  $eb c, dup code-here - 1- c, drop           \ jmp back (rel8)
  \ found: skip delimiter
  code-here r@ - r> 1- code-buf + c!          \ patch je found (relative)
  $48 c, $ff c, $c6 c,                        \ inc rsi (skip delim)
  \ done: calculate result
  code-here r@ - r> 1- code-buf + c!          \ patch jge done (relative)
  $48 c, $89 c, $34 c, $25 c, rt->in d,       \ mov [rt->in], rsi (update >in)
  $4c c, $89 c, $c3 c,                        \ mov rbx, r8 (start → NOS)
  $48 c, $81 c, $c3 c, rt-source-buf d,       \ add rbx, rt-source-buf (addr)
  $48 c, $89 c, $f0 c,                        \ mov rax, rsi
  $4c c, $29 c, $c0 c,                        \ sub rax, r8 (len = >in - start)
  \ If we skipped delim, len is one less
  $48 c, $39 c, $d6 c,                        \ cmp rsi, rdx (at end?)
  $74 c, 3 c,                                 \ je skip_dec
  $48 c, $ff c, $c8 c,                        \ dec rax (don't count delim)
  1 stack-depth +! ;                          \ result is 2 cells

: gen-word ( -- )
  \ ( char -- c-addr ) parse word, return counted string
  \ Uses gen-parse then copies to word-buf with count
  gen-parse
  \ Stack: rbx=addr, rax=len. Copy to word-buf.
  $48 c, $89 c, $c1 c,                        \ mov rcx, rax (len for rep movsb)
  $48 c, $89 c, $de c,                        \ mov rsi, rbx (source addr)
  $48 c, $bf c, rt-word-buf 1+ q,             \ mov rdi, rt-word-buf+1 (dest)
  $f3 c, $a4 c,                               \ rep movsb
  \ Store count byte
  $48 c, $88 c, $04 c, $25 c, rt-word-buf d,  \ mov [rt-word-buf], al
  \ Return address of counted string (drop NOS, set TOS)
  $48 c, $b8 c, rt-word-buf q,                \ mov rax, rt-word-buf
  -1 stack-depth +! ;                         \ result is 1 cell

: gen-accept ( -- )
  \ ( addr u1 -- u2 ) read line from stdin into buffer
  \ rax=max, rbx=addr. Result: rax=actual count
  1 has-io !
  stack-depth @ 3 >= if $51 c, then           \ push rcx if needed
  $48 c, $89 c, $da c,                        \ mov rdx, rbx (max count to rdx)
  $48 c, $89 c, $c6 c,                        \ mov rsi, rax (actually swap: rsi=addr)
  \ Oops - rax=u1 (max), rbx=addr. Need: rsi=addr, rdx=max
  \ Redo: rbx=addr→rsi, rax=max→rdx
  $48 c, $89 c, $c2 c,                        \ mov rdx, rax (max)
  $48 c, $89 c, $de c,                        \ mov rsi, rbx (addr)
  $31 c, $c0 c,                               \ xor eax, eax (sys_read=0)
  $31 c, $ff c,                               \ xor edi, edi (stdin=0)
  $0f c, $05 c,                               \ syscall
  \ rax = bytes read. Strip trailing newline if present.
  $48 c, $85 c, $c0 c,                        \ test rax, rax
  $7e c, 12 c,                                \ jle done (no bytes)
  $48 c, $8d c, $7c c, $06 c, $ff c,          \ lea rdi, [rsi+rax-1]
  $80 c, $3f c, 10 c,                         \ cmp byte [rdi], 10 (newline?)
  $75 c, 3 c,                                 \ jne done
  $48 c, $ff c, $c8 c,                        \ dec rax
  \ done: rax = count (without newline)
  stack-depth @ 3 >= if $59 c, then           \ pop rcx if needed
  -1 stack-depth +! ;                         \ result is 1 cell (was 2)

: gen-refill ( -- )
  \ ( -- flag ) read line into SOURCE buffer, reset >IN
  \ Returns true if line was read, false on EOF
  1 has-io !
  push-tos
  \ Read into rt-source-buf
  $48 c, $be c, rt-source-buf q,              \ mov rsi, rt-source-buf
  $ba c, SOURCE-SIZE d,                       \ mov edx, SOURCE-SIZE
  $31 c, $c0 c,                               \ xor eax, eax (sys_read=0)
  $31 c, $ff c,                               \ xor edi, edi (stdin=0)
  $0f c, $05 c,                               \ syscall
  \ rax = bytes read. Store in rt-source-len.
  $48 c, $89 c, $04 c, $25 c, rt-source-len d, \ mov [rt-source-len], rax
  \ Reset >in to 0
  $48 c, $c7 c, $04 c, $25 c, rt->in d, 0 d,  \ mov qword [rt->in], 0
  \ Return true if rax > 0
  $48 c, $85 c, $c0 c,                        \ test rax, rax
  $0f c, $9f c, $c0 c,                        \ setg al
  $48 c, $0f c, $b6 c, $c0 c, ;               \ movzx rax, al

\ Forward reference helpers for code generation
: >mark ( -- addr ) code-here 0 c, ;
: >resolve ( addr -- ) code-here over - 1- swap code-buf + c! ;
\ 32-bit store for near jump patching
: d! ( n addr -- )
  over $FF and over c! 1+
  over 8 rshift $FF and over c! 1+
  over 16 rshift $FF and over c! 1+
  swap 24 rshift swap c! ;
\ Near (32-bit) forward reference helpers for long jumps
: >mark32 ( -- addr ) code-here 0 d, ;
: >resolve32 ( addr -- ) code-here over - 4 - swap code-buf + d! ;

: gen-find ( -- )  \ ( addr u -- xt flag | 0 0 )
  $49 c, $89 c, $da c,  $49 c, $89 c, $c3 c,  \ r10=rbx(addr) r11=rax(len)
  $4c c, $8b c, $0c c, $25 c, rt-dict-count d, \ r9=[count]
  $4d c, $85 c, $c9 c,  $74 c, >mark          \ jz not_found
  $49 c, $83 c, $fb c, 24 c,  $7d c, >mark    \ jge not_found (len>=24)
  $31 c, $ff c,  $49 c, $b8 c, rt-dict-buf q, \ edi=0 r8=dict
  code-here                                   \ loop:
  $48 c, $6b c, $f7 c, 40 c,  $4c c, $01 c, $c6 c, \ rsi=r8+rdi*40
  $42 c, $80 c, $3c c, $1e c, 0 c,  $75 c, >mark   \ name[len]!=0? next
  $57 c,  $4c c, $89 c, $d9 c,  $4c c, $89 c, $d7 c, \ push rdi, rcx=len rdi=input
  $48 c, $87 c, $f7 c,  $f3 c, $a6 c,  $5f c, \ xchg cmpsb pop
  $75 c, >mark                                \ jne next
  \ found: recompute entry from index (rdi preserved)
  $48 c, $6b c, $f7 c, 40 c,  $4c c, $01 c, $c6 c, \ rsi=r8+rdi*40
  $48 c, $8b c, $5e c, 24 c,  $8b c, $46 c, 32 c, \ rbx=[xt] rax=[flags] (eax - 4 bytes)
  $48 c, $83 c, $f8 c, 1 c,  $48 c, $c7 c, $c0 c, -1 d,  \ cmp 1, mov -1
  $75 c, 7 c,  $48 c, $c7 c, $c0 c, 1 d,  $eb c, >mark   \ jne end, mov 1, jmp end
  >r >resolve >resolve                        \ next: resolve jne's
  $ff c, $c7 c,  $4c c, $39 c, $cf c,         \ inc cmp
  $7c c, code-here - 1- c,                    \ jl loop
  >resolve >resolve                           \ not_found:
  $31 c, $c0 c,  $31 c, $db c,                \ rax=0 rbx=0
  r> >resolve ;                               \ end:

: gen-execute ( -- )  \ ( xt -- )
  $ff c, $d0 c,                               \ call rax
  -1 stack-depth +! ;

: emit-rt-find ( -- )
  \ Emit FIND as a callable runtime function. Save address.
  $400000 176 + code-here + rt-find-addr !
  gen-find
  $c3 c, ;

variable rt-parse-addr  0 rt-parse-addr !

: emit-rt-parse ( -- )
  \ Emit parse-word as callable runtime function.
  \ Returns: rbx=addr, rax=len (0 if empty/EOF)
  $400000 176 + code-here + rt-parse-addr !

  \ Load source state
  $48 c, $8b c, $34 c, $25 c, rt->in d,       \ mov rsi, [rt->in]
  $48 c, $8b c, $3c c, $25 c, rt-source-len d, \ mov rdi, [rt-source-len]
  $4c c, $8b c, $04 c, $25 c, rt-source-addr d, \ mov r8, [rt-source-addr]

  \ Skip leading spaces: S: ()
  code-here                                   \ S: (skip_loop)
  $48 c, $39 c, $fe c,                        \ cmp rsi, rdi
  $7d c, >mark                                \ jge eof  S: (skip_loop eof_mark)
  $41 c, $0f c, $b6 c, $04 c, $30 c,          \ movzx eax, byte [r8+rsi]
  $3c c, 32 c,                                \ cmp al, 32
  $75 c, >mark                                \ jne found_char  S: (skip_loop eof_mark found_mark)
  $48 c, $ff c, $c6 c,                        \ inc rsi
  \ backward jump to skip_loop: need skip_loop which is 3rd on stack
  $eb c, 2 pick code-here - 1- c,             \ jmp skip_loop
  \ found_char: save word start
  >resolve                                    \ patch found_char  S: (skip_loop eof_mark)
  $49 c, $89 c, $f1 c,                        \ mov r9, rsi (word start)

  \ Scan to space/end: reuse stack position for scan_loop
  code-here swap rot drop                     \ S: (scan_loop eof_mark) - replace skip_loop
  $48 c, $39 c, $fe c,                        \ cmp rsi, rdi
  $7d c, >mark                                \ jge word_done  S: (scan_loop eof_mark done1)
  $41 c, $0f c, $b6 c, $04 c, $30 c,          \ movzx eax, byte [r8+rsi]
  $3c c, 32 c,                                \ cmp al, 32
  $74 c, >mark                                \ je word_done  S: (scan_loop eof_mark done1 done2)
  $48 c, $ff c, $c6 c,                        \ inc rsi
  \ backward jump to scan_loop
  $eb c, 3 pick code-here - 1- c,             \ jmp scan_loop
  \ word_done:
  >resolve >resolve                           \ patch both done marks  S: (scan_loop eof_mark)

  \ Update >IN and compute result
  $48 c, $89 c, $34 c, $25 c, rt->in d,       \ mov [rt->in], rsi
  $4c c, $89 c, $cb c,                        \ mov rbx, r9 (word start)
  $4c c, $01 c, $c3 c,                        \ add rbx, r8 (addr in rbx)
  $48 c, $89 c, $f0 c,                        \ mov rax, rsi
  $4c c, $29 c, $c8 c,                        \ sub rax, r9 (len in rax)
  $c3 c,                                      \ ret

  \ eof: return 0
  >resolve                                    \ patch eof_mark  S: (scan_loop)
  $31 c, $c0 c,                               \ xor eax, eax
  $31 c, $db c,                               \ xor ebx, ebx
  $c3 c,                                      \ ret
  drop ;                                      \ clean up scan_loop

: gen-interpret-body ( -- )
  \ INTERPRET loop - parse words, find them, execute them
  code-here >r                                \ R: ( loop )
  $49 c, $b8 c, rt-parse-addr @ q,            \ mov r8, rt-parse-addr
  $41 c, $ff c, $d0 c,                        \ call r8  -> rbx=addr, rax=len
  $48 c, $85 c, $c0 c,                        \ test rax, rax
  $74 c, >mark                                \ jz done
  $49 c, $b8 c, rt-find-addr @ q,             \ mov r8, rt-find-addr
  $41 c, $ff c, $d0 c,                        \ call r8  -> rbx=xt, rax=flag (or 0,0)
  $48 c, $85 c, $db c,                        \ test rbx, rbx
  $74 c, >mark                                \ jz not_found
  $ff c, $d3 c,                               \ call rbx (execute found word)
  $eb c, r@ code-here - 1- c,                 \ jmp loop
  >resolve                                    \ not_found:
  $eb c, r@ code-here - 1- c,                 \ jmp loop
  >resolve                                    \ done:
  r> drop ;

: gen-interpret ( -- )  \ ( -- )
  gen-interpret-body
  $c3 c,                                      \ ret
  0 stack-depth ! ;

: gen-evaluate ( -- )  \ ( addr u -- )
  \ Save old input state to x86 stack
  $48 c, $8b c, $3c c, $25 c, rt-source-addr d,  \ mov rdi, [rt-source-addr]
  $57 c,                                         \ push rdi
  $48 c, $8b c, $3c c, $25 c, rt-source-len d,   \ mov rdi, [rt-source-len]
  $57 c,                                         \ push rdi
  $48 c, $8b c, $3c c, $25 c, rt->in d,          \ mov rdi, [rt->in]
  $57 c,                                         \ push rdi
  \ Set new input state: addr in rbx, len in rax
  $48 c, $89 c, $1c c, $25 c, rt-source-addr d,  \ mov [rt-source-addr], rbx
  $48 c, $89 c, $04 c, $25 c, rt-source-len d,   \ mov [rt-source-len], rax
  $48 c, $c7 c, $04 c, $25 c, rt->in d, 0 d,     \ mov qword [rt->in], 0
  \ Run INTERPRET loop (inline, no ret)
  gen-interpret-body
  \ Restore old input state
  $5f c,                                         \ pop rdi
  $48 c, $89 c, $3c c, $25 c, rt->in d,          \ mov [rt->in], rdi
  $5f c,                                         \ pop rdi
  $48 c, $89 c, $3c c, $25 c, rt-source-len d,   \ mov [rt-source-len], rdi
  $5f c,                                         \ pop rdi
  $48 c, $89 c, $3c c, $25 c, rt-source-addr d,  \ mov [rt-source-addr], rdi
  -2 stack-depth +! ;                            \ consumed addr u

: gen-quit ( -- )
  \ ( -- ) Main REPL loop: begin refill while interpret repeat
  1 has-io !
  code-here >r                                   \ R: ( loop )
  gen-refill                                     \ ( flag )
  $48 c, $85 c, $c0 c,                          \ test rax, rax
  $74 c, >mark                                   \ jz done
  pop-tos                                        \ consume flag
  gen-interpret-body                             \ interpret
  $eb c, r@ code-here - 1- c,                   \ jmp loop
  >resolve                                       \ done:
  r> drop
  0 stack-depth ! ;

: gen-abort ( -- )
  \ ( -- ) Clear stacks, call QUIT
  \ Reset data stack: r15 = 0x900000 (same as gen-prologue)
  $49 c, $bf c, $800000 $100000 + q,            \ mov r15, 0x900000
  gen-quit ;

: gen-throw ( -- )
  \ ( n -- ) Exit with code n (simplified THROW for bootstrapping)
  $48 c, $89 c, $c7 c,                          \ mov rdi, rax
  $b8 c, 60 d,                                  \ mov eax, 60 (exit syscall)
  $0f c, $05 c,                                 \ syscall
  -1 stack-depth +! ;

: gen-include ( -- )
  \ Parse filename, slurp file, evaluate contents
  1 has-io !
  $49 c, $b8 c, rt-parse-addr @ q,             \ mov r8, rt-parse-addr
  $41 c, $ff c, $d0 c,                         \ call r8  -> rbx=addr, rax=len
  2 stack-depth !
  gen-slurp-file                                \ ( addr u -- addr2 u2 )
  gen-evaluate ;                                \ ( addr2 u2 -- )

\ ============================================================
\ STRING COMPARE
\ ============================================================

here constant static-strings-start
2048 allot
here constant static-strings-end
static-strings-start constant str-ptr
variable str-ptr-cur  static-strings-start str-ptr-cur !

: s, ( addr u -- static-addr u )
  dup str-ptr-cur @ + static-strings-end < invert if
    ." Static string overflow" cr 1 throw
  then
  dup >r
  str-ptr-cur @ swap
  move
  str-ptr-cur @ r>
  dup str-ptr-cur +! ;

: str= ( addr1 u1 addr2 u2 -- flag )
  rot over <> if 2drop drop false exit then
  begin
    dup 0= if drop 2drop true exit then
    >r over c@ over c@ <> if 2drop r> drop false exit then
    1+ swap 1+ swap r> 1-
  again ;

s" dup" s, 2constant $dup
s" dup2" s, 2constant $dup2
s" drop" s, 2constant $drop
s" swap" s, 2constant $swap
s" over" s, 2constant $over
s" rot" s, 2constant $rot
s" nip" s, 2constant $nip
s" tuck" s, 2constant $tuck
s" 2dup" s, 2constant $2dup
s" 2drop" s, 2constant $2drop
s" +" s, 2constant $+
s" -" s, 2constant $-
s" *" s, 2constant $*
s" /" s, 2constant $/
s" mod" s, 2constant $mod
s" negate" s, 2constant $negate
s" 1+" s, 2constant $1+
s" 1-" s, 2constant $1-
s" nos+" s, 2constant $nos+
s" 2+" s, 2constant $2+
s" 2-" s, 2constant $2-
s" tuck+" s, 2constant $tuck+
s" and" s, 2constant $and
s" or" s, 2constant $or
s" xor" s, 2constant $xor
s" invert" s, 2constant $invert
s" abs" s, 2constant $abs
s" 2*" s, 2constant $2*
s" 2/" s, 2constant $2/
s" =" s, 2constant $=
s" <>" s, 2constant $<>
s" <" s, 2constant $<
s" >" s, 2constant $>
s" <if" s, 2constant $<if
s" >if" s, 2constant $>if
s" =if" s, 2constant $=if
s" 0<if" s, 2constant $0<if
s" 0=if" s, 2constant $0=if
s" <=" s, 2constant $<=
s" >=" s, 2constant $>=
s" 0=" s, 2constant $0=
s" 0<" s, 2constant $0<
s" 0>" s, 2constant $0>
s" 0<>" s, 2constant $0<>
s" @" s, 2constant $@
s" !" s, 2constant $!
s" c@" s, 2constant $c@
s" c!" s, 2constant $c!
s" ." s, 2constant $.
s" cr" s, 2constant $cr
s" emit" s, 2constant $emit
s" type" s, 2constant $type
s" if" s, 2constant $if
s" else" s, 2constant $else
s" then" s, 2constant $then
s" begin" s, 2constant $begin
s" until" s, 2constant $until
s" 0=until" s, 2constant $0=until
s" nzloop" s, 2constant $nzloop
s" 1-nzloop" s, 2constant $1-nzloop
s" while" s, 2constant $while
s" repeat" s, 2constant $repeat
s" again" s, 2constant $again
s" do" s, 2constant $do
s" ?do" s, 2constant $?do
s" loop" s, 2constant $loop
s" +loop" s, 2constant $+loop
s" i" s, 2constant $i
s" j" s, 2constant $j
s" leave" s, 2constant $leave
s" unloop" s, 2constant $unloop
s" within" s, 2constant $within
s" count" s, 2constant $count
s" recurse" s, 2constant $recurse
s" recursive" s, 2constant $recursive
s" exit" s, 2constant $exit
s" >r" s, 2constant $>r
s" r>" s, 2constant $r>
s" r@" s, 2constant $r@
s" 2>r" s, 2constant $2>r
s" 2r>" s, 2constant $2r>
s" 2r@" s, 2constant $2r@
s" min" s, 2constant $min
s" max" s, 2constant $max
s" lshift" s, 2constant $lshift
s" rshift" s, 2constant $rshift
s" +!" s, 2constant $+!
s" cells" s, 2constant $cells
s" cell+" s, 2constant $cell+
s" variable" s, 2constant $variable
s" constant" s, 2constant $constant
s" create" s, 2constant $create
s" allot" s, 2constant $allot
s" here" s, 2constant $here
s" s>d" s, 2constant $s>d
s" um*" s, 2constant $um*
s" m*" s, 2constant $m*
s" um/mod" s, 2constant $um/mod
s" sm/rem" s, 2constant $sm/rem
s" fm/mod" s, 2constant $fm/mod
s" d+" s, 2constant $d+
s" d-" s, 2constant $d-
s" ," s, 2constant $,
s" space" s, 2constant $space
s" -rot" s, 2constant $-rot
s" ?dup" s, 2constant $?dup
s" 2swap" s, 2constant $2swap
s" 2over" s, 2constant $2over
s" depth" s, 2constant $depth
s" pick" s, 2constant $pick
s" move" s, 2constant $move
s" fill" s, 2constant $fill
s" [char]" s, 2constant $[char]
s" base" s, 2constant $base
s" <#" s, 2constant $<#
s" hold" s, 2constant $hold
s" sign" s, 2constant $sign
s" #" s, 2constant $#
s" #s" s, 2constant $#s
s" #>" s, 2constant $#>
s" c," s, 2constant $c,
s" s," s, 2constant $s,
s" 2constant" s, 2constant $2constant
s" key" s, 2constant $key
s" bl" s, 2constant $bl
s" true" s, 2constant $true
s" false" s, 2constant $false
s" decimal" s, 2constant $decimal
s" chars" s, 2constant $chars
s" char+" s, 2constant $char+
s" /mod" s, 2constant $/mod
s" */mod" s, 2constant $*/mod
s" */" s, 2constant $*/
s" spaces" s, 2constant $spaces
s" u." s, 2constant $u.
s" u<" s, 2constant $u<
s" :" s, 2constant $:
s" ;" s, 2constant $;
s" main" s, 2constant $main
s" source" s, 2constant $source
s" >in" s, 2constant $>in
s" parse" s, 2constant $parse
s" word" s, 2constant $word
s" accept" s, 2constant $accept
s" refill" s, 2constant $refill
s" find" s, 2constant $find
s" execute" s, 2constant $execute
s" '" s, 2constant $'
s" >body" s, 2constant $>body
s" [" s, 2constant $[
s" ]" s, 2constant $]
s" literal" s, 2constant $literal
s" postpone" s, 2constant $postpone
s" does>" s, 2constant $does>
s" interpret" s, 2constant $interpret
s" evaluate" s, 2constant $evaluate
s" quit" s, 2constant $quit
s" abort" s, 2constant $abort
s" throw" s, 2constant $throw
s" include" s, 2constant $include
s" r/o" s, 2constant $r/o
s" w/o" s, 2constant $w/o
s" r/w" s, 2constant $r/w
s" argc" s, 2constant $argc
s" argv" s, 2constant $argv
s" open-file" s, 2constant $open-file
s" create-file" s, 2constant $create-file
s" close-file" s, 2constant $close-file
s" read-file" s, 2constant $read-file
s" write-file" s, 2constant $write-file
s" slurp-file" s, 2constant $slurp-file
s" include" s, 2constant $include

\ ============================================================
\ TOKENIZER
\ ============================================================

: skip-ws ( -- )
  begin
    input-pos @ input-len @ >= if exit then
    input-buf input-pos @ + c@
    dup 32 <= if
      drop 1 input-pos +!
    else
      drop exit
    then
  again ;

: skip-line ( -- )
  begin
    input-pos @ input-len @ >= if exit then
    input-buf input-pos @ + c@ 10 = if
      1 input-pos +! exit
    then
    1 input-pos +!
  again ;

create token-buf 64 allot
variable token-len

: get-token ( -- addr u | 0 0 )
  skip-ws
  input-pos @ input-len @ >= if 0 0 exit then
  input-buf input-pos @ + c@ 92 = if  \ 92 = backslash
    skip-line
    recurse exit
  then
  input-buf input-pos @ + c@ 40 = if  \ 40 = open paren
    input-pos @ 1+ input-len @ < if
      input-buf input-pos @ 1+ + c@ 32 <= if
        1 input-pos +!
        begin
          input-pos @ input-len @ >= if 0 0 exit then
          input-buf input-pos @ + c@ 41 = if  \ 41 = close paren
            1 input-pos +! recurse exit
          then
          1 input-pos +!
        again
      then
    then
  then
  0 token-len !
  begin
    input-pos @ input-len @ >= if
      token-buf token-len @ exit
    then
    input-buf input-pos @ + c@
    dup 32 <= if
      drop token-buf token-len @ exit
    then
    token-buf token-len @ + c!
    1 token-len +!
    1 input-pos +!
    token-len @ 63 >= if token-buf token-len @ exit then
  again ;

: gen-' ( -- )  \ ( "name" -- xt ) - parse at compile time, emit xt as literal
  get-token dict-find ?dup if
    dict-addr @ $FFFFFFFF and $4000B0 +
    push-tos  $48 c, $b8 c, q,  1 stack-depth +!
  else ." ' unknown word" cr 1 throw then ;

: gen->body ( -- )  \ ( xt -- addr ) extract data addr from CREATE'd word
  $48 c, $8b c, $40 c, 2 c, ;  \ mov rax, [rax+2]

\ gen-postpone is inlined in compile-builtin (needs to call compile-builtin)

\ ============================================================
\ NUMBER PARSER
\ ============================================================

variable num-val
variable num-neg

: parse-number ( addr u -- n true | false )
  0 num-val !
  0 num-neg !
  dup 0= if 2drop false exit then
  over c@ [char] - = if
    1 num-neg !
    1 /string
  then
  dup 0= if 2drop false exit then
  over c@ [char] $ = if
    1 /string
    dup 0= if 2drop false exit then
    begin
      dup 0> while
      over c@
      dup [char] 0 >= over [char] 9 <= and if
        [char] 0 -
      else
        dup [char] a >= over [char] f <= and if
          [char] a - 10 +
        else
          dup [char] A >= over [char] F <= and if
            [char] A - 10 +
          else
            drop 2drop false exit
          then
        then
      then
      num-val @ 16 * + num-val !
      1 /string
    repeat
    2drop
    num-neg @ if num-val @ negate else num-val @ then
    true exit
  then
  begin
    dup 0> while
    over c@
    dup [char] 0 >= over [char] 9 <= and if
      [char] 0 -
      num-val @ 10 * + num-val !
    else
      drop 2drop false exit
    then
    1 /string
  repeat
  2drop
  num-neg @ if num-val @ negate else num-val @ then
  true ;

\ ============================================================
\ BUILT-IN WORDS TABLE
\ ============================================================

: install-builtins ( -- )
  s" dup" dict-add     ' gen-dup dict-entry 24 - dict-addr @ dict-entry 24 - dict-addr !
  ;

: flush-pending ( -- )
  pending-call @ ?dup if gen-call then
  0 pending-call ! ;

: discard-pending ( -- )
  0 pending-call ! ;

\ OPTIMIZATION 1+2 dispatch: constant folding and literal-op fusion.
\ For binary ops (+, -, *, and, or, xor):
\   ct-depth >= 2 → fold both constants (pure compile-time eval)
\   ct-depth == 1 → fuse: emit one x86 immediate instruction
\   ct-depth == 0 → normal two-operand codegen
\ For unary ops (negate, 1+, 1-, 2*, 2/, invert, abs):
\   ct-depth >= 1 → fold the constant
\   ct-depth == 0 → normal codegen
\ For everything else: ct-flush first, then normal codegen.
: compile-builtin ( addr u -- found? )
  \ ---- Stack manipulation: flush ct-stack first ----
  2dup $dup str= if 2drop flush-swap ct-flush
    1 dup-pending ! true exit then
  2dup $dup2 str= if 2drop flush-swap ct-flush gen-dup2 true exit then
  2dup $drop str= if 2drop flush-swap
    ct-depth @ 0> if ct-pop drop else ct-flush gen-drop then
    true exit then
  2dup $swap str= if 2drop ct-flush
    swap-pending @ if  0 swap-pending !
    else  1 swap-pending !
    then true exit then
  2dup $over str= if 2drop flush-swap ct-flush gen-over true exit then
  2dup $rot str= if 2drop flush-swap ct-flush gen-rot true exit then
  2dup $nip str= if 2drop flush-swap ct-flush gen-nip true exit then
  2dup $tuck str= if 2drop flush-swap ct-flush gen-tuck true exit then
  2dup $2dup str= if 2drop flush-swap ct-flush gen-2dup true exit then
  2dup $2drop str= if 2drop flush-swap ct-flush gen-2drop true exit then
  2dup $-rot str= if 2drop flush-swap ct-flush gen--rot true exit then
  2dup $?dup str= if 2drop flush-swap ct-flush gen-?dup true exit then
  2dup $2swap str= if 2drop flush-swap ct-flush gen-2swap true exit then
  2dup $2over str= if 2drop flush-swap ct-flush gen-2over true exit then
  2dup $depth str= if 2drop flush-swap ct-flush gen-depth true exit then
  2dup $pick str= if 2drop flush-swap ct-flush gen-pick true exit then
  \ ---- Binary arithmetic with fold/fuse ----
  2dup $+ str= if 2drop flush-cmp
    ct-depth @ 0= if 0 swap-pending ! else flush-swap then
    ct-depth @ 2 >= if ct-pop ct-pop + ct-push
    else ct-depth @ 1 = if ct-pop flush-pending gen-add-imm
    else flush-pending gen-add then then true exit then
  2dup $- str= if 2drop flush-cmp
    swap-pending @ if
      ct-depth @ 0= if
        0 swap-pending ! flush-pending gen-rsub
      else flush-swap
        ct-depth @ 2 >= if ct-pop ct-pop swap - ct-push
        else ct-depth @ 1 = if ct-pop flush-pending gen-sub-imm
        else flush-pending gen-sub then then
      then
    else
      ct-depth @ 2 >= if ct-pop ct-pop swap - ct-push
      else ct-depth @ 1 = if ct-pop flush-pending gen-sub-imm
      else flush-pending gen-sub then then
    then true exit then
  2dup $* str= if 2drop flush-cmp
    ct-depth @ 0= if 0 swap-pending ! else flush-swap then
    ct-depth @ 2 >= if ct-pop ct-pop * ct-push
    else ct-depth @ 1 = if
      ct-pop dup power-of-2? if
        flush-pending log2 gen-lshift-imm
      else flush-pending gen-mul-imm then
    else flush-pending gen-mul then then true exit then
  2dup $/ str= if 2drop flush-swap
    ct-depth @ 2 >= if ct-pop ct-pop swap / ct-push
    else ct-flush flush-pending gen-div then true exit then
  2dup $mod str= if 2drop flush-swap
    ct-depth @ 2 >= if ct-pop ct-pop swap mod ct-push
    else ct-depth @ 1 = if
      ct-pop dup power-of-2? if
        1- flush-pending gen-and-imm   \ n mod 2^k = n and (2^k - 1)
      else ct-push ct-flush flush-pending gen-mod then
    else ct-flush flush-pending gen-mod then then true exit then
  2dup $/mod str= if 2drop flush-swap
    ct-depth @ 2 >= if ct-pop ct-pop swap /mod >r ct-push r> ct-push
    else ct-flush flush-pending gen-/mod then true exit then
  2dup $*/mod str= if 2drop flush-swap ct-flush flush-pending gen-*/mod true exit then
  2dup $*/ str= if 2drop flush-swap ct-flush flush-pending gen-*/ true exit then
  \ ---- Unary arithmetic with fold ----
  2dup $negate str= if 2drop flush-cmp
    swap-pending @ if
      ct-depth @ 0> if ct-pop negate ct-push else $48 c, $f7 c, $db c, then
    else
      ct-depth @ 0> if ct-pop negate ct-push else gen-negate then
    then true exit then
  \ OPTIMIZATION 7: swap absorption for 1+/1- (see header)
  \ swap-pending: inc/dec rbx instead of rax. DO NOT clear swap-pending here.
  \ Registers are still logically swapped after absorbing.
  2dup $1+ str= if 2drop flush-cmp
    swap-pending @ if
      ct-depth @ 0> if ct-pop 1+ ct-push else $48 c, $ff c, $c3 c, then  \ inc rbx
    else
      ct-depth @ 0> if ct-pop 1+ ct-push else gen-1+ then
    then true exit then
  2dup $1- str= if 2drop flush-cmp
    swap-pending @ if
      ct-depth @ 0> if ct-pop 1- ct-push else $48 c, $ff c, $cb c, then  \ dec rbx
    else
      ct-depth @ 0> if ct-pop 1- ct-push else gen-1- then
    then true exit then
  2dup $nos+ str= if 2drop flush-swap ct-flush gen-nos+ true exit then
  2dup $2+ str= if 2drop flush-swap
    ct-depth @ 0> if ct-pop 2 + ct-push else gen-2+ then true exit then
  2dup $2- str= if 2drop flush-swap
    ct-depth @ 0> if ct-pop 2 - ct-push else gen-2- then true exit then
  2dup $tuck+ str= if 2drop flush-swap ct-flush gen-tuck+ true exit then
  \ ---- Bitwise with fold/fuse ----
  2dup $and str= if 2drop flush-cmp
    ct-depth @ 0= if 0 swap-pending ! else flush-swap then
    ct-depth @ 2 >= if ct-pop ct-pop and ct-push
    else ct-depth @ 1 = if ct-pop flush-pending gen-and-imm
    else flush-pending gen-and then then true exit then
  2dup $or str= if 2drop flush-cmp
    ct-depth @ 0= if 0 swap-pending ! else flush-swap then
    ct-depth @ 2 >= if ct-pop ct-pop or ct-push
    else ct-depth @ 1 = if ct-pop flush-pending gen-or-imm
    else flush-pending gen-or then then true exit then
  2dup $xor str= if 2drop flush-cmp
    ct-depth @ 0= if 0 swap-pending ! else flush-swap then
    ct-depth @ 2 >= if ct-pop ct-pop xor ct-push
    else ct-depth @ 1 = if ct-pop flush-pending gen-xor-imm
    else flush-pending gen-xor then then true exit then
  2dup $invert str= if 2drop flush-swap
    ct-depth @ 0> if ct-pop invert ct-push else gen-invert then true exit then
  2dup $abs str= if 2drop flush-swap
    ct-depth @ 0> if ct-pop abs ct-push else gen-abs then true exit then
  2dup $2* str= if 2drop flush-swap
    ct-depth @ 0> if ct-pop 2 * ct-push else gen-2* then true exit then
  2dup $2/ str= if 2drop flush-swap
    ct-depth @ 0> if ct-pop 2 / ct-push else gen-2/ then true exit then
  \ ---- Comparison: flush, then normal ----
  2dup $= str= if 2drop flush-swap ct-flush flush-pending gen-= true exit then
  2dup $<> str= if 2drop flush-swap ct-flush flush-pending gen-<> true exit then
  2dup $< str= if 2drop flush-swap ct-flush flush-pending gen-< true exit then
  2dup $> str= if 2drop flush-swap ct-flush flush-pending gen-> true exit then
  2dup $<= str= if 2drop flush-swap ct-flush flush-pending gen-<= true exit then
  2dup $>= str= if 2drop flush-swap ct-flush flush-pending gen->= true exit then
  2dup $u< str= if 2drop flush-swap ct-flush flush-pending gen-u< true exit then
  2dup $0= str= if 2drop dup-pending @ 0 dup-pending !
    flush-swap ct-flush flush-pending
    if 1 cmp-pending ! else gen-0= then true exit then
  2dup $0< str= if 2drop dup-pending @ 0 dup-pending !
    flush-swap ct-flush flush-pending
    if 3 cmp-pending ! else gen-0< then true exit then
  2dup $0> str= if 2drop dup-pending @ 0 dup-pending !
    flush-swap ct-flush flush-pending
    if 2 cmp-pending ! else gen-0> then true exit then
  2dup $0<> str= if 2drop flush-swap ct-flush flush-pending gen-0<> true exit then
  2dup $min str= if 2drop flush-swap ct-flush flush-pending gen-min true exit then
  2dup $max str= if 2drop flush-swap ct-flush flush-pending gen-max true exit then
  2dup $lshift str= if 2drop flush-swap ct-flush flush-pending gen-lshift true exit then
  2dup $rshift str= if 2drop flush-swap ct-flush flush-pending gen-rshift true exit then
  \ ---- Double-cell: flush, then normal ----
  2dup $s>d str= if 2drop flush-swap ct-flush flush-pending gen-s>d true exit then
  2dup $um* str= if 2drop flush-swap ct-flush flush-pending gen-um* true exit then
  2dup $m* str= if 2drop flush-swap ct-flush flush-pending gen-m* true exit then
  2dup $um/mod str= if 2drop flush-swap ct-flush flush-pending gen-um/mod true exit then
  2dup $sm/rem str= if 2drop flush-swap ct-flush flush-pending gen-sm/rem true exit then
  2dup $fm/mod str= if 2drop flush-swap ct-flush flush-pending gen-fm/mod true exit then
  2dup $d+ str= if 2drop flush-swap ct-flush flush-pending gen-d+ true exit then
  2dup $d- str= if 2drop flush-swap ct-flush flush-pending gen-d- true exit then
  \ ---- Memory: flush, then normal ----
  2dup $@ str= if 2drop flush-swap ct-flush gen-@ true exit then
  2dup $! str= if 2drop flush-swap ct-flush flush-pending gen-! true exit then
  2dup $c@ str= if 2drop flush-swap ct-flush gen-c@ true exit then
  2dup $c! str= if 2drop flush-swap ct-flush flush-pending gen-c! true exit then
  2dup $+! str= if 2drop flush-swap ct-flush flush-pending gen-+! true exit then
  2dup $cells str= if 2drop flush-swap ct-flush gen-cells true exit then
  2dup $cell+ str= if 2drop flush-swap ct-flush gen-cell+ true exit then
  2dup $bl str= if 2drop flush-swap 32 ct-push true exit then
  2dup $true str= if 2drop flush-swap -1 ct-push true exit then
  2dup $false str= if 2drop flush-swap 0 ct-push true exit then
  2dup $chars str= if 2drop true exit then   \ nop on byte-addressed machine
  2dup $char+ str= if 2drop flush-swap
    ct-depth @ 0> if ct-pop 1+ ct-push else gen-1+ then true exit then
  2dup $decimal str= if 2drop flush-swap ct-flush
    \ mov qword [rt-base], 10
    $48 c, $c7 c, $04 c, $25 c, rt-base d, 10 d,
    true exit then
  \ ---- I/O: flush, then normal ----
  2dup $. str= if 2drop flush-swap ct-flush flush-pending gen-dot true exit then
  2dup $cr str= if 2drop flush-swap ct-flush gen-cr true exit then
  2dup $emit str= if 2drop flush-swap ct-flush flush-pending gen-emit true exit then
  2dup $type str= if 2drop flush-swap ct-flush flush-pending gen-type true exit then
  2dup $space str= if 2drop flush-swap ct-flush gen-space true exit then
  2dup $spaces str= if 2drop flush-swap ct-flush flush-pending gen-spaces true exit then
  2dup $u. str= if 2drop flush-swap ct-flush flush-pending gen-u. true exit then
  2dup $key str= if 2drop flush-swap ct-flush gen-key true exit then
  \ ---- Memory operations ----
  2dup $move str= if 2drop flush-swap ct-flush flush-pending gen-move true exit then
  2dup $fill str= if 2drop flush-swap ct-flush flush-pending gen-fill true exit then
  \ ---- File I/O ----
  2dup $r/o str= if 2drop flush-swap ct-flush gen-r/o true exit then
  2dup $w/o str= if 2drop flush-swap ct-flush gen-w/o true exit then
  2dup $r/w str= if 2drop flush-swap ct-flush gen-r/w true exit then
  2dup $open-file str= if 2drop flush-swap ct-flush flush-pending gen-open-file true exit then
  2dup $create-file str= if 2drop flush-swap ct-flush flush-pending gen-create-file true exit then
  2dup $close-file str= if 2drop flush-swap ct-flush flush-pending gen-close-file true exit then
  2dup $read-file str= if 2drop flush-swap ct-flush flush-pending gen-read-file true exit then
  2dup $write-file str= if 2drop flush-swap ct-flush flush-pending gen-write-file true exit then
  2dup $slurp-file str= if 2drop flush-swap ct-flush flush-pending gen-slurp-file true exit then
  2dup $include str= if 2drop flush-swap ct-flush flush-pending gen-include true exit then
  2dup $argc str= if 2drop flush-swap ct-flush gen-argc true exit then
  2dup $argv str= if 2drop flush-swap ct-flush flush-pending gen-argv true exit then
  \ ---- Pictured Numeric Output ----
  2dup $base str= if 2drop flush-swap ct-flush gen-base true exit then
  2dup $<# str= if 2drop flush-swap ct-flush gen-<# true exit then
  2dup $hold str= if 2drop flush-swap ct-flush flush-pending gen-hold true exit then
  2dup $sign str= if 2drop flush-swap ct-flush flush-pending gen-sign true exit then
  2dup $# str= if 2drop flush-swap ct-flush flush-pending gen-# true exit then
  2dup $#s str= if 2drop flush-swap ct-flush flush-pending gen-#s true exit then
  2dup $#> str= if 2drop flush-swap ct-flush flush-pending gen-#> true exit then
  \ ---- Input Parsing ----
  2dup $source str= if 2drop flush-swap ct-flush gen-source true exit then
  2dup $>in str= if 2drop flush-swap ct-flush gen->in true exit then
  2dup $parse str= if 2drop flush-swap ct-flush flush-pending gen-parse true exit then
  2dup $word str= if 2drop flush-swap ct-flush flush-pending gen-word true exit then
  2dup $accept str= if 2drop flush-swap ct-flush flush-pending gen-accept true exit then
  2dup $refill str= if 2drop flush-swap ct-flush gen-refill true exit then
  2dup $find str= if 2drop flush-swap ct-flush flush-pending gen-find true exit then
  2dup $execute str= if 2drop flush-swap ct-flush flush-pending gen-execute true exit then
  2dup $' str= if 2drop flush-swap ct-flush flush-pending gen-' true exit then
  2dup $postpone str= if
    2drop flush-swap ct-flush flush-pending
    get-token dict-find ?dup if
      dup dict-flags @ dup 3 rshift $F and call-nargs !
                          7 rshift $F and call-rets !
      dict-addr @ $FFFFFFFF and
      gen-call
    else ." POSTPONE: word not in dictionary (builtins not yet supported)" cr 1 throw then
    true exit
  then
  2dup $>body str= if 2drop flush-swap ct-flush flush-pending gen->body true exit then
  2dup $interpret str= if 2drop flush-swap ct-flush gen-interpret true exit then
  2dup $evaluate str= if 2drop flush-swap ct-flush flush-pending gen-evaluate true exit then
  2dup $quit str= if 2drop flush-swap ct-flush gen-quit true exit then
  2dup $abort str= if 2drop flush-swap ct-flush gen-abort true exit then
  2dup $throw str= if 2drop flush-swap ct-flush flush-pending gen-throw true exit then
  2dup $[ str= if 2drop flush-swap ct-flush 0 state ! true exit then
  2dup $] str= if 2drop 1 state ! true exit then
  2dup $literal str= if 2drop interp-val @ ct-push true exit then
  2dup $does> str= if
    2drop flush-swap ct-flush flush-pending
    \ NOTE: DOES> only works with direct CREATE usage, not inside defining words.
    \ Our CREATE parses at compile-time, so `: constant create , does> @ ;` fails.
    \ Use the builtin CONSTANT/VARIABLE words instead.
    last-create-code @ ?dup 0= if
      ." DOES> without CREATE" cr 1 throw
    then
    dup 10 + code-buf + $e9 swap c!            \ ret -> jmp
    code-here swap - 15 -                      \ offset (32-bit)
    last-create-code @ 11 + code-buf +         \ patch address ( offset addr )
    over $FF and over c!                       \ byte 0
    swap 8 rshift swap
    over $FF and over 1+ c!                    \ byte 1
    swap 8 rshift swap
    over $FF and over 2 + c!                   \ byte 2
    swap 8 rshift swap
    3 + c!                                     \ byte 3
    \ Clear $800 flag so word is called, not inlined
    last-create-dict @ 32 * dict-buf + 28 +
    dup @ $800 invert and swap !
    1 stack-depth !
    true exit
  then
  \ ---- Control flow: flush, then normal ----
  2dup $if str= if 2drop flush-swap ct-flush flush-pending stack-depth @ cf-push gen-if cf-push true exit then
  2dup $<if str= if 2drop flush-swap ct-flush flush-pending stack-depth @ cf-push gen-<if cf-push true exit then
  2dup $>if str= if 2drop flush-swap ct-flush flush-pending stack-depth @ cf-push gen->if cf-push true exit then
  2dup $=if str= if 2drop flush-swap ct-flush flush-pending stack-depth @ cf-push gen-=if cf-push true exit then
  2dup $0<if str= if 2drop flush-swap ct-flush flush-pending stack-depth @ cf-push gen-0<if cf-push true exit then
  2dup $0=if str= if 2drop flush-swap ct-flush flush-pending stack-depth @ cf-push gen-0=if cf-push true exit then
  2dup $else str= if 2drop flush-swap ct-flush cf-pop gen-else cf-pop >r stack-depth @ cf-push r> stack-depth ! cf-push true exit then
  2dup $then str= if 2drop flush-swap ct-flush cf-pop gen-then cf-pop stack-depth ! true exit then
  2dup $begin str= if 2drop flush-swap ct-flush gen-begin cf-push true exit then
  \ OPTIMIZATION 8: save dup-pending/cmp-pending BEFORE flush-swap clears them.
  \ Non-fused path emits gen-dup if dup was pending (e.g. dup until without 0>).
  2dup $until str= if 2drop cmp-pending @ 0 cmp-pending ! dup-pending @ 0 dup-pending !
    flush-swap ct-flush flush-pending
    swap ?dup if nip cf-pop swap gen-until-fused
    else if gen-dup then cf-pop gen-until then true exit then
  2dup $0=until str= if 2drop flush-swap ct-flush flush-pending cf-pop gen-0=until true exit then
  2dup $nzloop str= if 2drop flush-swap ct-flush flush-pending cf-pop gen-nzloop true exit then
  2dup $1-nzloop str= if 2drop flush-swap ct-flush flush-pending cf-pop gen-1-nzloop true exit then
  \ OPTIMIZATION 8: same pattern as until. See comment above $until handler.
  2dup $while str= if 2drop cmp-pending @ 0 cmp-pending ! dup-pending @ 0 dup-pending !
    flush-swap ct-flush flush-pending
    swap ?dup if nip cf-pop swap gen-while-fused cf-push cf-push
    else if gen-dup then cf-pop gen-while cf-push cf-push then true exit then
  2dup $repeat str= if 2drop flush-swap ct-flush cf-pop cf-pop gen-repeat true exit then
  2dup $again str= if 2drop flush-swap ct-flush cf-pop gen-again true exit then
  2dup $do str= if 2drop flush-swap
    \ OPTIMIZATION 10: save known trip count and code position.
    \ Only optimize when: ct-depth=2 (just limit/start) AND stack-depth=0 (no runtime values).
    \ This ensures nested loops and loops with accumulators are NOT optimized away.
    ct-depth @ 2 = stack-depth @ 0= and if
      ct-stack 0 cells + @   \ limit (first/deeper)
      ct-stack 1 cells + @   \ start (second/shallower)
      - do-depth @ cells do-trip + !      \ trip = limit - start
    else -1 do-depth @ cells do-trip + ! then
    ct-flush flush-pending
    code-here do-depth @ cells do-origin + !    \ save rewind point AFTER flush
    stack-depth @ do-depth @ cells do-sdepth + !  \ save stack depth AFTER flush
    gen-do true exit then
  2dup $?do str= if 2drop flush-swap
    \ ?DO: like DO but skips if limit <= start
    ct-depth @ 2 = stack-depth @ 0= and if
      ct-stack 0 cells + @
      ct-stack 1 cells + @
      - do-depth @ cells do-trip + !
    else -1 do-depth @ cells do-trip + ! then
    ct-flush flush-pending
    code-here do-depth @ cells do-origin + !
    stack-depth @ do-depth @ cells do-sdepth + !
    gen-?do true exit then
  2dup $loop str= if 2drop flush-swap ct-flush gen-loop true exit then
  2dup $+loop str= if 2drop flush-swap ct-flush flush-pending gen-+loop true exit then
  2dup $i str= if 2drop flush-swap ct-flush gen-i true exit then
  2dup $j str= if 2drop flush-swap ct-flush gen-j true exit then
  2dup $leave str= if 2drop flush-swap ct-flush gen-leave true exit then
  2dup $unloop str= if 2drop flush-swap ct-flush gen-unloop true exit then
  2dup $within str= if 2drop flush-swap ct-flush flush-pending gen-within true exit then
  2dup $count str= if 2drop flush-swap ct-flush gen-count true exit then
  2dup $recursive str= if 2drop 1 is-recursive ! true exit then
  2dup $recurse str= if
    2drop flush-swap ct-flush 1 is-recursive ! code-here tail-recurse ! gen-recurse true exit then
  2dup $>r str= if 2drop flush-swap ct-flush gen->r true exit then
  2dup $r> str= if 2drop flush-swap ct-flush gen-r> true exit then
  2dup $r@ str= if 2drop flush-swap ct-flush gen-r@ true exit then
  2dup $2>r str= if 2drop flush-swap ct-flush gen-2>r true exit then
  2dup $2r> str= if 2drop flush-swap ct-flush gen-2r> true exit then
  2dup $2r@ str= if 2drop flush-swap ct-flush gen-2r@ true exit then
  2dup $exit str= if 2drop flush-swap ct-flush gen-ret true exit then
  2dup $here str= if 2drop flush-swap data-here @ ct-push true exit then
  2dup $[char] str= if 2drop flush-swap get-token dup 0> if drop c@ ct-push else 2drop then true exit then
  dup 2 = if over dup c@ [char] . = swap 1+ c@ [char] " = and if
    2drop flush-swap ct-flush gen-dotquote true exit
  then then
  dup 2 = if over dup c@ [char] s = swap 1+ c@ [char] " = and if
    2drop flush-swap ct-flush gen-s" true exit
  then then
  2drop false ;

\ ============================================================
\ COMPILER
\ ============================================================
\ WORD INFO TABLE (for double-pass forward reference resolution)
\ ============================================================
\ OPTIMIZATION 3: DOUBLE PASS infrastructure.
\ Pass 1 (scan-all) walks the source and records every : definition's
\ name, nargs (from stack comment), and flags (has-io, is-void).
\ Pass 2 (compile-all) uses info-find to resolve forward references
\ with correct nargs instead of defaulting to 1.
\ Without this, calling a 2-arg word before its definition would
\ generate wrong register save/restore code.
\ Tests: 1030-fwd-ref-simple through 1033-fwd-ref-nargs, 1048-fwd-ref-void

: info-entry ( i -- addr ) 40 * info-buf + ;
: info-name= ( addr u entry -- flag )
  >r dup 23 > if 2drop r> drop false exit then
  r@ over + c@ 0 <> if r> drop 2drop false exit then
  r> swap 0 ?do
    2dup i + c@ swap i + c@ <> if 2drop false unloop exit then
  loop 2drop true ;

: info-find ( addr u -- entry | 0 )
  info-count @ 0 ?do
    2dup i info-entry info-name= if
      2drop i info-entry unloop exit
    then
  loop
  2drop 0 ;

variable scan-nargs
variable scan-rets
variable scan-void
variable scan-io

: scan-skip-ws ( -- )
  begin
    input-pos @ input-len @ >= if exit then
    input-buf input-pos @ + c@ dup 32 <= swap 0 > and if
      1 input-pos +!
    else exit then
  again ;

: scan-stack-comment ( -- )
  scan-skip-ws
  input-pos @ input-len @ >= if exit then
  input-buf input-pos @ + c@ [char] ( <> if exit then
  input-pos @ 1+ input-len @ >= if exit then
  input-buf input-pos @ 1+ + c@ 32 > if exit then
  1 input-pos +!
  1 scan-void !  0 scan-nargs !  0 scan-rets !
  0 >r
  begin
    scan-skip-ws
    input-pos @ input-len @ >= if r> drop exit then
    input-buf input-pos @ + c@
    dup [char] ) = if drop r> drop 1 input-pos +! exit then
    dup [char] - = if
      input-pos @ 1+ input-len @ < if
        input-buf input-pos @ 1+ + c@ [char] - = if
          drop 2 input-pos +! r> drop 1 >r
        else
          begin input-pos @ input-len @ < while input-buf input-pos @ + c@ 32 > while 1 input-pos +! repeat then
          r> dup if 1 scan-rets +! 0 scan-void ! then dup 0= if drop 1 scan-nargs +! 0 then >r
        then
      else
        begin input-pos @ input-len @ < while input-buf input-pos @ + c@ 32 > while 1 input-pos +! repeat then
        r> dup if 1 scan-rets +! 0 scan-void ! then dup 0= if drop 1 scan-nargs +! 0 then >r
      then
    else
      drop
      begin input-pos @ input-len @ < while input-buf input-pos @ + c@ 32 > while 1 input-pos +! repeat then
      r> dup if 1 scan-rets +! 0 scan-void ! then dup 0= if drop 1 scan-nargs +! 0 then >r
    then
  again ;

: scan-body-io ( -- )
  0 scan-io !
  begin
    get-token dup 0= if 2drop exit then
    2dup $; str= if 2drop exit then
    2dup $. str= if 1 scan-io ! then
    2dup $cr str= if 1 scan-io ! then
    2dup $emit str= if 1 scan-io ! then
    2dup $space str= if 1 scan-io ! then
    2dup $spaces str= if 1 scan-io ! then
    2dup $u. str= if 1 scan-io ! then
    2dup $key str= if 1 scan-io ! then
    2dup $type str= if 1 scan-io ! then
    \ Increment call-count for referenced words (if already in info-buf)
    2dup info-find ?dup if
      28 + dup c@ 1+ 255 min swap c!   \ increment call-count (max 255)
    then
    2drop
  again ;

create scan-name-buf 24 allot
variable scan-name-len
variable scan-body-pos

: scan-add-info ( -- )
  info-count @ INFO-MAX >= if exit then
  info-count @ info-entry >r
  r@ 40 0 fill                                     \ clear entire entry
  scan-name-buf r@ scan-name-len @ dup 23 > if drop 23 then move
  scan-nargs @ 1 max r@ 24 + c!                    \ nargs at 24 (1 byte)
  scan-rets @ 1 max r@ 25 + c!                      \ rets at 25 (1 byte)
  scan-io @ r@ 26 + c!                             \ flags at 26 (1 byte)
  \ call-count at 28 (2 bytes) - already 0 from fill
  scan-body-pos @ r@ 30 + !                        \ body-pos at 30 (4 bytes)
  \ code-addr at 34 (4 bytes) - already 0 from fill, set when compiled
  r> drop
  1 info-count +! ;

: scan-all ( -- )
  0 info-count !
  begin
    get-token dup 0= if 2drop exit then
    2dup $: str= if
      2drop
      get-token dup 0= if 2drop exit then
      dup scan-name-len !
      dup 23 > if drop 23 then
      scan-name-buf over 0 fill
      scan-name-buf swap move
      1 scan-void !  1 scan-nargs !  0 scan-io !
      scan-stack-comment
      input-pos @ scan-body-pos !                  \ save body start position
      scan-body-io
      scan-add-info
    else
      2drop
    then
  again ;

: compile-token ( addr u -- )
  0 tail-recurse !
  \ Check info-buf first (has call-count for future inlining)
  2dup info-find ?dup if
    dup 24 + c@ call-nargs !
    dup 25 + c@ dup 0= if drop 1 then call-rets !
    34 + @                             \ ( addr u code-addr )
    ?dup if                            \ code-addr != 0: word is compiled
      -rot 2drop                       \ ( code-addr )
      flush-swap ct-flush
      gen-call exit
    then
    \ code-addr = 0: not yet compiled, fall through to dict-find or forward ref
  then
  \ User definitions - dict-find handles $800 stubs for variables/constants
  2dup dict-find ?dup if
    nip nip flush-swap ct-flush
    \ INLINE: variable/constant stubs → ct-push the literal value
    dup dict-flags @ $800 and if
      dict-addr @ $FFFFFFFF and 2 + code-buf + @ ct-push
      exit
    then
    dup dict-flags @ dup if
      dup 3 rshift $F and call-nargs !
      7 rshift $F and call-rets !
    else
      drop arg-count @ call-nargs !
      1 call-rets !
    then
    dict-addr @ gen-call exit
  then
  \ No user definition — try builtins and numbers
  2dup compile-builtin if 2drop exit then
  2dup parse-number if
    nip nip
    dup-pending @ if gen-dup  0 dup-pending ! then
    ct-push exit
  then
  \ Forward reference — not yet defined
  flush-swap ct-flush
  2dup info-find ?dup if
    dup 24 + c@ call-nargs !
    dup 25 + c@ dup 0= if drop 1 then call-rets !
    drop
  else
    1 call-nargs !  1 call-rets !
  then
  $e8 c,
  0 d,
  code-here add-fixup ;

variable is-void  0 is-void !
variable ret-count 1 ret-count !

: skip-ws-only ( -- )
  begin
    input-pos @ input-len @ >= if exit then
    input-buf input-pos @ + c@ dup 32 <= swap 0 > and if
      1 input-pos +!
    else exit then
  again ;

: parse-stack-comment ( -- )
  skip-ws-only
  input-pos @ input-len @ >= if exit then
  input-buf input-pos @ + c@ [char] ( <> if exit then
  input-pos @ 1+ input-len @ >= if exit then
  input-buf input-pos @ 1+ + c@ 32 > if exit then
  1 input-pos +!
  1 is-void !
  0 arg-count !
  0 ret-count !
  0 >r
  begin
    skip-ws-only
    input-pos @ input-len @ >= if r> drop exit then
    input-buf input-pos @ + c@
    dup [char] ) = if drop r> drop 1 input-pos +! exit then
    dup [char] - = if
      input-pos @ 1+ input-len @ < if
        input-buf input-pos @ 1+ + c@ [char] - = if
          drop 2 input-pos +! r> drop 1 >r
        else
          drop  \ drop the '-' char
          begin input-buf input-pos @ + c@ 32 > while 1 input-pos +! repeat
          r> dup if 0 is-void ! 1 ret-count +! then dup 0= if drop 1 arg-count +! 0 then >r
        then
      else
        drop  \ drop the '-' char
        begin input-buf input-pos @ + c@ 32 > while 1 input-pos +! repeat
        r> dup if 0 is-void ! 1 ret-count +! then dup 0= if drop 1 arg-count +! 0 then >r
      then
    else
      drop
      begin input-buf input-pos @ + c@ 32 > while 1 input-pos +! repeat
      r> dup if 0 is-void ! 1 ret-count +! then dup 0= if drop 1 arg-count +! 0 then >r
    then
  again ;

: start-def ( addr u -- )
  0 ct-depth !
  2dup dict-add
  \ Store code-addr in info-buf if word exists there
  2dup info-find ?dup if 34 + code-here swap ! then
  2drop
  code-here current-word-addr !
  0 has-io !
  0 is-void !
  0 do-depth !
  1 arg-count !
  1 ret-count !
  parse-stack-comment
  arg-count @ stack-depth !
  stack-depth @ start-depth !
  1 state ! ;

: end-def ( -- )
  flush-swap ct-flush
  tail-recurse @ ?dup if
    code-pos !
    gen-tail-recurse
    0 tail-recurse !
  else
    gen-ret
  then
  \ Store word size in flags bits 16-23 (max 255)
  code-here current-word-addr @ - 255 min 16 lshift
  1 or
  has-io @ 1 lshift or
  is-void @ 0= if 4 or then
  arg-count @ 3 lshift or
  ret-count @ 7 lshift or
  dict-buf dict-count @ 1- 32 * + dict-flags !
  0 is-recursive !
  0 state ! ;

\ INCLUDE support - temp buffer for splice operation
create inc-input 8192 allot

: compile-word ( addr u -- )
  2dup $: str= if
    2drop
    get-token
    dup 0= if 2drop ." Expected name after :" cr 1 throw then
    start-def exit
  then
  2dup $; str= if
    2drop end-def exit
  then
  2dup $variable str= if
    2drop get-token
    dup 0= if 2drop ." Expected name after variable" cr 1 throw then
    dict-add
    $48 c, $b8 c, data-here @ q,
    $c3 c,
    \ flags: 0 args (0<<3), 1 ret (1<<7), non-void (4), inline ($800)
    5 128 or $800 or dict-buf dict-count @ 1- 32 * + dict-flags !
    8 data-here +!
    exit
  then
  2dup $constant str= if
    2drop get-token
    dup 0= if 2drop ." Expected name after constant" cr 1 throw then
    dict-add
    \ Use interp-val (set by preceding number in interpret mode)
    $48 c, $b8 c, interp-val @ q,
    $c3 c,
    5 128 or $800 or dict-buf dict-count @ 1- 32 * + dict-flags !
    exit
  then
  2dup $create str= if
    2drop get-token
    dup 0= if 2drop ." Expected name after create" cr 1 throw then
    dict-add
    dict-count @ 1- last-create-dict !  \ save dict index for DOES>
    code-here last-create-code !        \ save code pos for DOES>
    $48 c, $b8 c, data-here @ q,        \ mov rax, data-addr (10 bytes)
    $c3 c,                              \ ret (will be patched by DOES>)
    0 c, 0 c, 0 c, 0 c,                 \ padding for jmp rel32
    5 128 or $800 or dict-buf dict-count @ 1- 32 * + dict-flags !
    exit
  then
  2dup $allot str= if
    2drop interp-val @ data-here +! exit
  then
  2dup $, str= if
    2drop interp-val @ data-here @ 8 add-init 8 data-here +! exit
  then
  2dup $c, str= if
    2drop interp-val @ data-here @ 1 add-init 1 data-here +! exit
  then
  \ s, in interpret mode: string already in data area from s", just keep addr/len
  dup 2 = if over dup c@ [char] s = swap 1+ c@ [char] , = and if
    2drop exit  \ s" already stored string, addr/len in interp-val2/interp-val
  then then
  \ 2constant: create word that pushes two values (addr len for strings)
  2dup $2constant str= if
    2drop get-token
    dup 0= if 2drop ." Expected name after 2constant" cr 1 throw then
    dict-add
    \ Generate: mov rbx,rax; mov rax,val1; mov rbx,rax; mov rax,val2; ret
    $48 c, $89 c, $c3 c,                      \ mov rbx, rax (save TOS)
    $48 c, $b8 c, interp-val2 @ q,            \ mov rax, addr
    $48 c, $89 c, $c3 c,                      \ mov rbx, rax (addr -> NOS)
    $48 c, $b8 c, interp-val @ q,             \ mov rax, len
    $c3 c,                                    \ ret
    5 256 or $800 or dict-buf dict-count @ 1- 32 * + dict-flags !  \ 0 args, 2 rets, inline
    exit
  then
  2dup $] str= if 2drop 1 state ! exit then
  2dup $literal str= if 2drop interp-val @ ct-push exit then
  2dup $include str= if
    2drop get-token
    dup 0= if ." include-noname " 2drop 1 throw then
    slurp-file  ( file-addr file-len )
    dup 0= if ." include-notfound " 2drop 1 throw then
    \ Splice file into input buffer at current position
    input-len @ input-pos @ - ( file-addr file-len remaining )
    input-buf input-pos @ + inc-input 2 pick move  \ save remaining to temp
    2 pick input-buf input-pos @ + 3 pick move     \ copy file content
    input-pos @ 2 pick +                           \ new-end = pos + file-len
    inc-input input-buf 2 pick + 2 pick move       \ append remaining
    + input-len !  2drop                           \ new-len = new-end + remaining
    exit
  then
  state @ if
    compile-token
  else
    \ Handle ! in interpret mode: ( val addr -- ) record as init
    2dup $! str= if
      2drop interp-val2 @ interp-val @ 8 add-init exit
    then
    \ Interpret-mode arithmetic
    2dup $+ str= if 2drop interp-val2 @ interp-val @ + interp-val ! exit then
    2dup $- str= if 2drop interp-val2 @ interp-val @ - interp-val ! exit then
    2dup $* str= if 2drop interp-val2 @ interp-val @ * interp-val ! exit then
    2dup $/ str= if 2drop interp-val2 @ interp-val @ / interp-val ! exit then
    2dup $cells str= if 2drop interp-val @ 8 * interp-val ! exit then
    2dup $here str= if 2drop interp-val @ interp-val2 ! data-here @ interp-val ! exit then
    \ s" in interpret mode: parse string into data area, return addr len
    dup 2 = if over dup c@ [char] s = swap 1+ c@ [char] " = and if
      2drop 1 input-pos +!
      data-here @                    \ save start addr
      0                              \ len counter
      begin
        input-pos @ input-len @ >= if true else
        input-buf input-pos @ + c@ [char] " = if
          1 input-pos +! true
        else
          input-buf input-pos @ + c@ data-here @ 1 add-init
          1 data-here +! 1 input-pos +! 1+ false
        then then
      until
      \ Stack: ( start-addr len )
      interp-val2 ! interp-val ! exit
    then then
    2dup parse-number if nip nip interp-val @ interp-val2 ! interp-val ! else
      \ Try dictionary lookup for constants
      2dup dict-find ?dup if
        nip nip
        dup dict-flags @ $800 and if
          \ Inline constant: extract value from stub
          interp-val @ interp-val2 !
          dict-addr @ $FFFFFFFF and 2 + code-buf + @ interp-val !
        else
          ." Unexpected word in interpret mode: " 2drop type cr
          1 throw
        then
      else
        ." Unexpected token in interpret mode: " type cr
        1 throw
      then
    then
  then ;

: compile-all ( -- )
  begin
    get-token
    dup 0= if 2drop exit then
    compile-word
  again ;

\ ============================================================
\ RUNTIME DICTIONARY INITIALIZATION
\ ============================================================

: emit-dict-count ( -- )
  \ mov qword [rt-dict-count], dict-count
  $48 c, $c7 c, $04 c, $25 c, rt-dict-count d, dict-count @ d, ;

: emit-dict-entry ( i -- )
  \ Emit code to initialize runtime dict entry i from compile-time entry i
  \ Runtime entry: name[24] + xt[8] + flags[8] = 40 bytes
  \ Compile-time:  name[24] + addr[4] + flags[4] = 32 bytes
  dup 32 * dict-buf +              \ ct-entry address
  swap 40 * rt-dict-buf +          \ rt-entry address
  \ Stack: ct-entry rt-entry

  \ Copy name qword 0 (bytes 0-7)
  over @ ?dup if
    $48 c, $bf c, q,               \ mov rdi, value
    $48 c, $89 c, $3c c, $25 c, dup d,  \ mov [rt-entry], rdi
  then

  \ Copy name qword 1 (bytes 8-15)
  over 8 + @ ?dup if
    $48 c, $bf c, q,
    $48 c, $89 c, $3c c, $25 c, dup 8 + d,
  then

  \ Copy name qword 2 (bytes 16-23)
  over 16 + @ ?dup if
    $48 c, $bf c, q,
    $48 c, $89 c, $3c c, $25 c, dup 16 + d,
  then

  \ Set xt = $400000 + 176 + code-offset
  over 24 + @ $FFFFFFFF and        \ code offset (4 bytes, masked)
  $400000 176 + +                  \ absolute code address
  $48 c, $bf c, q,                 \ mov rdi, xt
  $48 c, $89 c, $3c c, $25 c, dup 24 + d,  \ mov [rt-entry+24], rdi

  \ Set flags (if non-zero) - use mov dword since flags fit in 32 bits
  over 28 + @ $FFFFFFFF and        \ ct-entry rt-entry flags
  dup if
    $c7 c, $04 c, $25 c,
    over 32 + d,                   \ address: rt-entry+32
    d,                             \ value: flags
  else drop then

  2drop ;

: emit-dict-init ( -- )
  \ Emit code to initialize runtime dictionary from compile-time dictionary
  emit-dict-count
  dict-count @ 0 ?do i emit-dict-entry loop ;

\ ============================================================
\ MAIN
\ ============================================================

: load-file ( addr u -- )
  slurp-file
  dup INPUT-SIZE > if
    ." File too large" cr 1 throw
  then
  dup input-len !
  input-buf swap move
  0 input-pos ! ;

: compile-file ( src-addr src-u out-addr out-u -- )
  2swap load-file
  scan-all
  0 input-pos !
  0 code-pos !
  0 dict-count !
  0 cf-sp !
  0 state !
  0 fixup-count !
  0 ct-depth !
  DATA-BASE 45328 SLURP-SIZE + + data-here !
  gen-prologue
  emit-rt-parse
  emit-rt-find
  compile-all
  \ Check for unresolved forward references (resolved entries are zeroed)
  fixup-count @ ?dup if
    0 do
      i fixup-entry c@ if
        ." Unresolved: " i fixup-entry 24 type cr
        1 throw
      then
    loop
  then
  patch-start
  \ Reset compiler state for startup code
  0 stack-depth !  0 ct-depth !
  0 swap-pending !  0 dup-pending !  0 cmp-pending !
  \ Initialize base variable to 10
  $48 c, $c7 c, $04 c, $25 c, rt-base d, 10 d,  \ mov qword [rt-base], 10
  \ Initialize source buffer state for INTERPRET
  $48 c, $c7 c, $04 c, $25 c, rt->in d, 0 d,        \ mov qword [rt->in], 0
  $48 c, $c7 c, $04 c, $25 c, rt-source-len d, 0 d, \ mov qword [rt-source-len], 0
  $48 c, $bf c, rt-source-buf q,                    \ mov rdi, rt-source-buf
  $48 c, $89 c, $3c c, $25 c, rt-source-addr d,     \ mov [rt-source-addr], rdi
  \ Emit data initializations from , and c,
  init-count @ 0 ?do
    i init-entry >r
    r@ 16 + @ 8 = if
      \ mov rdi, value; mov qword [addr], rdi
      $48 c, $bf c, r@ 8 + @ q,
      $48 c, $89 c, $3c c, $25 c, r@ @ d,
    else
      \ mov byte [addr], value
      $c6 c, $04 c, $25 c, r@ @ d, r@ 8 + @ c,
    then
    r> drop
  loop
  emit-dict-init
  $main dict-find
  ?dup if
    0 call-nargs !  0 call-rets !
    dict-addr @ $FFFFFFFF and
    gen-call
  then
  gen-cr
  gen-epilogue
  elf-header
  write-elf ;

create src-name 64 allot
variable src-len
create out-name 64 allot
variable out-len

: save-src ( addr u -- ) dup src-len ! src-name swap move ;
: save-out ( addr u -- ) dup out-len ! out-name swap move ;
: get-src ( -- addr u ) src-name src-len @ ;
: get-out ( -- addr u ) out-name out-len @ ;

create cmd-buf 128 allot

: usage ( -- )
  ." Usage: sixth sixth.fs <input.fs> <output>" cr bye ;

: make-executable ( -- )
  s" chmod +x " cmd-buf swap move
  get-out cmd-buf 9 + swap move
  cmd-buf 9 out-len @ + 0 swap c!
  cmd-buf system drop ;

: main-entry ( -- )
  argc 4 < if usage then
  2 argv save-src
  3 argv save-out
  get-src get-out compile-file
  make-executable
  bye ;

main-entry
