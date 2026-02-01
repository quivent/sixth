\ sixth.fs - Sixth Native Compiler
\ Self-hosting: sixth sixth.fs sixth.fs → native sixth compiler
\ Then: ./sixth program.fs → native program
\
\ OPTIMIZATIONS IN THIS FILE (do not remove without understanding):
\
\   1. CONSTANT FOLDING - Evaluate literal arithmetic at compile time.
\      e.g. 3 7 + compiles as push 10 (zero instructions wasted).
\      Impl: ct-stack, ct-push/ct-pop/ct-flush, fold paths in compile-builtin.
\      Tests: 1000-1015, 1027-1029, 1035-1037, 1042, 1044-1046, 1049
\
\   2. LITERAL-OP FUSION - Fuse literal with runtime arithmetic into one x86 insn.
\      e.g. runtime_val 3 * emits imul rax,rax,3 instead of push+load+mul+pop.
\      Impl: gen-add-imm, gen-sub-imm, gen-mul-imm, gen-and-imm, gen-or-imm,
\            gen-xor-imm, fuse paths (ct-depth=1) in compile-builtin.
\      Tests: 1016-1021, 1034, 1043, 1047
\
\   3. DOUBLE PASS - Scan source first to build word-info table, then compile.
\      Resolves forward references with correct nargs instead of guessing 1.
\      Impl: scan-all, info-buf, info-find, scan-stack-comment, scan-body-io.
\      Tests: 1030-1033, 1048
\
\   4. DEAD CODE ELIMINATION - Pure void words called for side effects are tracked.
\      Impl: has-io, is-void, pending-call, pending-pure, dict-flags encoding.
\
\   5. TAIL-CALL OPTIMIZATION - recurse at end of definition becomes jmp.
\      Impl: tail-recurse variable, patch in end-def.
\
\   6. REGISTER-BASED STACK - stack-depth tracks TOS in rax/rbx/rcx/memory.
\      Avoids memory traffic for shallow stacks (depth <= 3).
\
\   7. SWAP ABSORPTION - Deferred swap cancels or absorbs into next op.
\      swap swap → nop. swap 1+ → inc rbx (operate on NOS, keep swapped).
\      Same for 1- (dec rbx), negate (neg rbx). Binary ops (+, -, etc.)
\      absorb swap by clearing swap-pending (operands are commutative in regs).
\      Impl: swap-pending variable, swap toggle in compile-builtin $swap,
\            rbx opcodes in $1+/$1-/$negate handlers, flush-cmp calls in
\            all binary/unary op handlers to flush dup-pending before absorbing.
\      CRITICAL: swap-pending stays SET after absorbing into 1+/1-/negate.
\      The registers are still logically swapped. Only a second swap clears it.
\
\   8. PEEPHOLE: DUP+CMP+WHILE/UNTIL FUSION - Fuses dup 0> while into one branch.
\      dup 0> while → test rax,rax; jle forward (3+6=9 bytes, no dup/cmp overhead).
\      Works for 0=, 0>, 0<. Skips test if last insn was dec/inc (flags already set).
\      dup without following cmp is preserved (emitted in non-fused path).
\      Impl: dup-pending, cmp-pending variables. flush-cmp emits deferred dup+cmp.
\            $dup sets dup-pending. $0>/$0=/$0< save dup-pending into cmp-pending.
\            $while/$until save both, take fused path if cmp-pending != 0.
\            gen-while-fused, gen-until-fused: emit condition + forward/backward jmp.
\            last-sets-flags?: detects dec/inc rax, skips redundant test rax,rax.
\      CRITICAL: while/until save dup-pending BEFORE flush-swap clears it.
\      Non-fused path must emit gen-dup if dup was pending (dup while without cmp).
\      Tests: 325-gcd, 861-peep-while-count, 950-multi-while-seq, 990-sum-1to100
\
\   9. WHILE/REPEAT FUSION + LOOP ELIMINATION - Three levels of optimization:
\      a) BACKWARD FUSION: adjacent while+repeat (no body between them) fuses
\         forward-jcc + jmp-backward into single backward-jcc. Inverts condition
\         byte (XOR 1: jle→jg, jnz→jz, jns→js). Saves 5 bytes + 1 branch/iter.
\      b) COUNTDOWN ELIMINATION: begin 1- dup 0> while repeat → xor eax,eax.
\         Recognizes 3-byte body (48 ff c8 = dec rax) as pure countdown-to-zero.
\      c) NOS++ ELIMINATION: begin swap 1+ swap 1- dup 0> while repeat
\         → add rbx,rax; xor eax,eax. Recognizes 6-byte body
\         (48 ff c3 48 ff c8 = inc rbx; dec rax) as accumulate+countdown.
\      Impl: gen-repeat checks code-here == orig (adjacent), then body-size
\            and byte patterns. Uses dup code-buf + >r / r@ for byte reads.
\      Result: arith-std 228ms→1ms, loop-std 222ms→1ms. Matches gcc -O2.
\      CRITICAL: byte reads use (dup code-buf +), NOT (code-buf dup +).
\      Stack is (orig dest). dup copies dest (TOS). over copies orig (wrong).

\ ============================================================
\ MISSING WORDS
\ >= <= missing from interpreter, now in lib/core.fs
\ <> exists in interpreter but redefined for clarity
\ ============================================================

: >= ( a b -- flag ) < 0= ;
: <= ( a b -- flag ) > 0= ;
: <> ( a b -- flag ) = 0= ;

\ ============================================================
\ BUFFERS
\ ============================================================

4096 constant CODE-SIZE
256 constant DICT-SIZE
4096 constant INPUT-SIZE

create code-buf CODE-SIZE allot
variable code-pos  0 code-pos !

create elf-buf 256 allot
variable elf-pos  0 elf-pos !

create input-buf INPUT-SIZE allot
variable input-len  0 input-len !
variable input-pos  0 input-pos !

\ Dictionary: 64 entries, 32 bytes each (24 name + 4 addr + 4 flags)
create dict-buf DICT-SIZE 32 * allot
variable dict-count  0 dict-count !

\ Control flow stack for forward references
create cf-stack 64 cells allot
variable cf-sp  0 cf-sp !

\ Data segment allocation (variables, create/allot)
$40A000 constant DATA-BASE
\ Reserved data segment layout:
\   +0:   base variable (8 bytes, initialized to 10)
\   +8:   pno-pos variable (8 bytes)
\   +16:  pno-buf (128 bytes)
\   +144: user data starts here
DATA-BASE constant rt-base         \ runtime address of base variable
DATA-BASE 8 + constant rt-pno-pos  \ runtime address of pno position
DATA-BASE 16 + constant rt-pno-buf \ runtime address of PNO buffer (128 bytes)
128 constant PNO-SIZE
variable data-here  DATA-BASE 144 + data-here !

\ Data initialization table (for , and c,)
64 constant INIT-MAX
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
variable tos-cached  1 tos-cached !  \ Track if TOS is in rax

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
variable pending-pure   0 pending-pure !   \ 1 if pending call is pure void
variable do-depth       0 do-depth !       \ nesting depth of do/loop (for exit cleanup)
create do-trip 8 cells allot              \ known trip count per nesting level (-1=unknown)
create do-origin 8 cells allot            \ code-here before gen-do emitted anything
create do-sdepth 8 cells allot            \ stack-depth before gen-do

\ Compile-time constant stack for literal folding/fusion
create ct-stack 8 cells allot
variable ct-depth  0 ct-depth !

\ Word info table for double-pass (Pass 1 scan)
64 constant INFO-MAX
create info-buf INFO-MAX 32 * allot   \ 24 name + 4 nargs + 4 flags
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
  $7f e, 69 e, 76 e, 70 e,
  2 e, 1 e, 1 e, 0 e,
  0 e8,
  2 e2, $3e e2, 1 e4,
  $400078 e8,
  64 e8, 0 e8, 0 e4,
  64 e2, 56 e2, 1 e2,
  0 e2, 0 e2, 0 e2,
  1 e4, 7 e4, 0 e8,
  $400000 e8, $400000 e8,
  120 code-pos @ + e8,
  $10000 e8,
  $1000 e8, ;

: write-elf ( addr u -- )
  w/o create-file throw >r
  elf-buf 120 r@ write-file throw
  code-buf code-pos @ r@ write-file throw
  r> close-file throw ;

\ ============================================================
\ CONTROL FLOW STACK
\ ============================================================

: cf-push ( n -- ) cf-stack cf-sp @ cells + !  1 cf-sp +! ;
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
    $49 c, $83 c, $ef c, 8 c,
    $49 c, $89 c, $0f c,
  then
  dup 2 >= if
    $48 c, $89 c, $d9 c,
  then
  drop
  $48 c, $89 c, $c3 c,
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

variable nos+-pending  0 nos+-pending !
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
  stack-depth @ 2 >= call-nargs @ 2 < and if $5b c, then
  stack-depth @ 3 >= call-nargs @ 3 < and if $59 c, then
  call-nargs @ 2 = call-rets @ 1 = and if
    stack-depth @ 3 >= if
      $48 c, $89 c, $cb c,
      stack-depth @ 4 >= if
        $49 c, $8b c, $0f c,
        $49 c, $83 c, $c7 c, 8 c,
      then
    then
    -1 stack-depth +!
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
  -1 do-depth +! ;

: gen-i ( -- )
  push-tos
  $4c c, $89 c, $e0 c, ;

: gen-j ( -- )
  push-tos
  $48 c, $8b c, $44 c, $24 c, 8 c, ;

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

\ ============================================================
\ PROLOGUE / EPILOGUE
\ ============================================================

variable start-jmp

: gen-prologue ( -- )
  $49 c, $bf c, $400000 $8000 + q,
  $48 c, $bd c, $400000 $F000 + q,
  $e9 c, code-here start-jmp ! 0 d, ;

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
  swap $400078 + swap             \ ( runtime-addr len )
  push-tos
  $48 c, $b8 c, swap q,          \ mov rax, addr
  push-tos
  $48 c, $b8 c, q, ;             \ mov rax, len

: gen-dotquote ( -- )
  gen-s"
  gen-type ;

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
s" loop" s, 2constant $loop
s" +loop" s, 2constant $+loop
s" i" s, 2constant $i
s" j" s, 2constant $j
s" recurse" s, 2constant $recurse
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
s" key" s, 2constant $key
s" bl" s, 2constant $bl
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
  input-buf input-pos @ + c@ [char] \ = if
    skip-line
    recurse exit
  then
  input-buf input-pos @ + c@  [char] ( = if
    input-pos @ 1+ input-len @ < if
      input-buf input-pos @ 1+ + c@ 32 <= if
        1 input-pos +!
        begin
          input-pos @ input-len @ >= if 0 0 exit then
          input-buf input-pos @ + c@ [char] ) = if
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
    else ct-depth @ 1 = if ct-pop flush-pending gen-mul-imm
    else flush-pending gen-mul then then true exit then
  2dup $/ str= if 2drop flush-swap
    ct-depth @ 2 >= if ct-pop ct-pop swap / ct-push
    else ct-flush flush-pending gen-div then true exit then
  2dup $mod str= if 2drop flush-swap
    ct-depth @ 2 >= if ct-pop ct-pop swap mod ct-push
    else ct-flush flush-pending gen-mod then true exit then
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
  \ ---- Pictured Numeric Output ----
  2dup $base str= if 2drop flush-swap ct-flush gen-base true exit then
  2dup $<# str= if 2drop flush-swap ct-flush gen-<# true exit then
  2dup $hold str= if 2drop flush-swap ct-flush flush-pending gen-hold true exit then
  2dup $sign str= if 2drop flush-swap ct-flush flush-pending gen-sign true exit then
  2dup $# str= if 2drop flush-swap ct-flush flush-pending gen-# true exit then
  2dup $#s str= if 2drop flush-swap ct-flush flush-pending gen-#s true exit then
  2dup $#> str= if 2drop flush-swap ct-flush flush-pending gen-#> true exit then
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
    \ OPTIMIZATION 10: save known trip count and code position before ct-flush.
    ct-depth @ 2 >= if
      ct-stack ct-depth @ 2 - cells + @   \ limit (deeper)
      ct-stack ct-depth @ 1 - cells + @   \ start (shallower)
      - do-depth @ cells do-trip + !      \ trip = limit - start
    else -1 do-depth @ cells do-trip + ! then
    code-here do-depth @ cells do-origin + !    \ save rewind point
    stack-depth @ do-depth @ cells do-sdepth + !  \ save stack depth
    ct-flush flush-pending gen-do true exit then
  2dup $loop str= if 2drop flush-swap ct-flush gen-loop true exit then
  2dup $+loop str= if 2drop flush-swap ct-flush flush-pending gen-+loop true exit then
  2dup $i str= if 2drop flush-swap ct-flush gen-i true exit then
  2dup $j str= if 2drop flush-swap ct-flush gen-j true exit then
  2dup $recurse str= if 2drop flush-swap ct-flush code-here tail-recurse ! gen-recurse true exit then
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

variable current-def  0 current-def !

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

: info-entry ( i -- addr ) 32 * info-buf + ;
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
  1 scan-void !  0 scan-nargs !
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
          r> dup if 0 scan-void ! then dup 0= if drop 1 scan-nargs +! 0 then >r
        then
      else
        begin input-pos @ input-len @ < while input-buf input-pos @ + c@ 32 > while 1 input-pos +! repeat then
        r> dup if 0 scan-void ! then dup 0= if drop 1 scan-nargs +! 0 then >r
      then
    else
      drop
      begin input-pos @ input-len @ < while input-buf input-pos @ + c@ 32 > while 1 input-pos +! repeat then
      r> dup if 0 scan-void ! then dup 0= if drop 1 scan-nargs +! 0 then >r
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
    2drop
  again ;

create scan-name-buf 24 allot
variable scan-name-len

: scan-add-info ( -- )
  info-count @ INFO-MAX >= if exit then
  info-count @ info-entry >r
  r@ 24 0 fill
  scan-name-buf r@ scan-name-len @ dup 23 > if drop 23 then move
  scan-nargs @ 1 max r@ 24 + !
  scan-io @ 1 lshift
  scan-void @ 0= if 4 or then
  r> 28 + !
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
      scan-body-io
      scan-add-info
    else
      2drop
    then
  again ;

: compile-token ( addr u -- )
  0 tail-recurse !
  \ User definitions override builtins (standard Forth semantics)
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
    dup 24 + @ call-nargs !
    1 call-rets !
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
          begin input-buf input-pos @ + c@ 32 > while 1 input-pos +! repeat
          r> dup if 0 is-void ! 1 ret-count +! then dup 0= if drop 1 arg-count +! 0 then >r
        then
      else
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
  dict-add
  code-here current-word-addr !
  0 has-io !
  0 is-void !
  0 do-depth !
  1 arg-count !
  1 ret-count !
  parse-stack-comment
  arg-count @ 1 max stack-depth !
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
  1
  has-io @ 1 lshift or
  is-void @ 0= if 4 or then
  arg-count @ 3 lshift or
  ret-count @ 7 lshift or
  dict-buf dict-count @ 1- 32 * + dict-flags !
  0 state ! ;

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
    get-token
    dup 0= if 2drop ." Expected value after constant name" cr 1 throw then
    parse-number if
      $48 c, $b8 c, q,
      $c3 c,
      5 128 or $800 or dict-buf dict-count @ 1- 32 * + dict-flags !
    else ." Bad constant value" cr 1 throw then
    exit
  then
  2dup $create str= if
    2drop get-token
    dup 0= if 2drop ." Expected name after create" cr 1 throw then
    dict-add
    $48 c, $b8 c, data-here @ q,
    $c3 c,
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
  state @ if
    compile-token
  else
    2dup parse-number if nip nip interp-val ! else
      ." Unexpected token in interpret mode: " type cr
      1 throw
    then
  then ;

: compile-all ( -- )
  begin
    get-token
    dup 0= if 2drop exit then
    compile-word
  again ;

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
  DATA-BASE 144 + data-here !
  gen-prologue
  compile-all
  patch-start
  \ Reset compiler state for startup code
  0 stack-depth !  0 ct-depth !
  0 swap-pending !  0 dup-pending !  0 cmp-pending !
  \ Initialize base variable to 10
  $48 c, $c7 c, $04 c, $25 c, rt-base d, 10 d,  \ mov qword [rt-base], 10
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
  $main dict-find
  ?dup if
    0 call-nargs !  0 call-rets !
    dict-addr @ gen-call
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
