\ prims.fs - Primitive code generators (Shannon Layer 2)
\ Pure codegen with no optimization logic.
\
\ Requires: asm.fs for x86-64 instructions
\ Requires: stack.fs for stack machine interface
\
\ Each word emits code for one Forth primitive.
\ The optimization layer (Layer 3) wraps these.

\ Dependencies: asm.fs and stack.fs must be loaded before this file
\ include compiler/shannon/asm.fs
\ include compiler/shannon/stack.fs

\ ============================================================
\ STACK MANIPULATION
\ ============================================================

: emit-dup ( -- )
  \ ( x -- x x ) duplicate TOS
  push-val ;

: emit-drop ( -- )
  \ ( x -- ) discard TOS
  pop-val ;

: emit-swap ( -- )
  \ ( x y -- y x ) exchange TOS and NOS
  tos nos xchg-rr ;

: emit-over ( -- )
  \ ( x y -- x y x ) copy NOS to TOS
  push-val
  nos tos mov-rr ;

: emit-rot ( -- )
  \ ( x y z -- y z x ) rotate 3rd to TOS
  tos third xchg-rr      \ z<->x: x y z -> z y x
  third nos xchg-rr ;    \ x<->y: z y x -> z x y? No...
  \ Actually: tos=z, nos=y, third=x
  \ After xchg tos,third: tos=x, nos=y, third=z
  \ After xchg third,nos: tos=x, nos=z, third=y
  \ Result: third=y, nos=z, tos=x = y z x. Correct!

: emit-nip ( -- )
  \ ( x y -- y ) discard NOS
  pop-nos-val ;

: emit-tuck ( -- )
  \ ( x y -- y x y ) copy TOS below NOS
  emit-swap emit-over ;

: emit-2dup ( -- )
  \ ( x y -- x y x y ) duplicate top pair
  push-val
  nos tos mov-rr         \ copy old NOS (now 3rd) to TOS
  push-val
  nos tos mov-rr ;       \ copy old TOS (now 3rd) to TOS

: emit-2drop ( -- )
  \ ( x y -- ) discard top pair
  pop-val pop-val ;

\ ============================================================
\ ARITHMETIC
\ ============================================================

: emit-add ( -- )
  \ ( x y -- x+y ) add NOS + TOS, result in TOS
  nos tos add-rr
  pop-nos-val ;

: emit-sub ( -- )
  \ ( x y -- x-y ) subtract NOS - TOS
  \ sub rbx, rax; mov rax, rbx
  tos nos sub-rr         \ nos = nos - tos
  nos tos mov-rr         \ tos = nos
  pop-nos-val ;

: emit-mul ( -- )
  \ ( x y -- x*y ) multiply NOS * TOS
  nos tos imul-rr
  pop-nos-val ;

: emit-negate ( -- )
  \ ( x -- -x ) negate TOS
  tos neg-r ;

: emit-1+ ( -- )
  \ ( x -- x+1 ) increment TOS
  tos inc-r ;

: emit-1- ( -- )
  \ ( x -- x-1 ) decrement TOS
  tos dec-r ;

\ ============================================================
\ FUSED IMMEDIATE OPERATIONS
\ ============================================================

: emit-add-imm ( n -- )
  \ ( x -- x+n ) add immediate to TOS
  tos add-ri ;

: emit-sub-imm ( n -- )
  \ ( x -- x-n ) subtract immediate from TOS
  tos sub-ri ;

: emit-mul-imm ( n -- )
  \ ( x -- x*n ) multiply TOS by immediate
  tos imul-ri ;

: emit-and-imm ( n -- )
  \ ( x -- x&n ) AND immediate with TOS
  tos and-ri ;

: emit-or-imm ( n -- )
  \ ( x -- x|n ) OR immediate with TOS
  tos or-ri ;

: emit-xor-imm ( n -- )
  \ ( x -- x^n ) XOR immediate with TOS
  tos xor-ri ;

: emit-lshift-imm ( n -- )
  \ ( x -- x<<n ) left shift TOS by n
  dup 1 = if
    drop tos tos add-rr  \ add rax,rax is shorter than shl rax,1
  else
    tos shl-ri
  then ;

\ ============================================================
\ COMPARISON
\ ============================================================

\ Helper: compare NOS to TOS, pop NOS
: cmp-nos-tos ( -- )
  \ cmp rbx, rax (compares nos TO tos, sets flags)
  tos nos cmp-rr
  pop-nos-val ;

\ Helper: set TOS to -1 or 0 based on SETcc byte
: emit-setcc ( cc -- )
  \ cc is the second byte of SETcc (e.g., $94 for SETZ)
  cmp-nos-tos
  $0f c, c, $c0 c,           \ SETcc al
  tos movzx-r8               \ movzx rax, al
  tos neg-r ;                \ neg rax (0->0, 1->-1)

: emit-= ( -- )
  \ ( x y -- flag ) -1 if equal, 0 otherwise
  $94 emit-setcc ;           \ SETZ

: emit-< ( -- )
  \ ( x y -- flag ) -1 if NOS < TOS (signed)
  $9c emit-setcc ;           \ SETL

: emit-> ( -- )
  \ ( x y -- flag ) -1 if NOS > TOS (signed)
  $9f emit-setcc ;           \ SETG

\ Helper: test TOS against itself, then SETcc
: emit-test-setcc ( cc -- )
  tos tos test-rr            \ test rax, rax
  $0f c, c, $c0 c,           \ SETcc al
  tos movzx-r8               \ movzx rax, al
  tos neg-r ;                \ neg rax

: emit-0= ( -- )
  \ ( x -- flag ) -1 if TOS is zero
  $94 emit-test-setcc ;      \ SETZ

: emit-0< ( -- )
  \ ( x -- flag ) -1 if TOS is negative
  \ Arithmetic right shift by 63 gives -1 for negative, 0 for non-negative
  63 tos sar-ri ;

: emit-0> ( -- )
  \ ( x -- flag ) -1 if TOS is positive (greater than zero)
  $9f emit-test-setcc ;      \ SETG (signed greater)

\ ============================================================
\ MEMORY
\ ============================================================

: emit-@ ( -- )
  \ ( addr -- x ) fetch 64-bit value from address in TOS
  0 tos tos mov-rm ;         \ mov rax, [rax]

: emit-! ( -- )
  \ ( x addr -- ) store x at addr
  \ addr=TOS=rax, x=NOS=rbx
  nos 0 tos mov-mr           \ mov [rax], rbx
  \ Now promote stack: drop both TOS and NOS
  pop-val                    \ rax = rbx (old NOS)
  pop-val ;                  \ rax = rcx (old 3rd)

: emit-c@ ( -- )
  \ ( addr -- c ) fetch byte from address in TOS
  0 tos tos movzx-rm8 ;      \ movzx rax, byte [rax]

: emit-c! ( -- )
  \ ( c addr -- ) store byte c at addr
  \ addr=TOS=rax, c=NOS=rbx
  nos 0 tos mov-mr8          \ mov byte [rax], bl
  pop-val pop-val ;

: emit-+! ( -- )
  \ ( n addr -- ) add n to [addr]
  \ addr=TOS=rax, n=NOS=rbx
  nos 0 tos add-mr           \ add [rax], rbx
  pop-val pop-val ;

: emit-move ( -- )
  \ ( src dst u -- ) copy u bytes from src to dst
  \ u=TOS=rax, dst=NOS=rbx, src=3rd=rcx
  \ rep movsb needs: rsi=src, rdi=dst, rcx=count
  $48 c, $89 c, $ce c,        \ mov rsi, rcx (src from 3rd)
  $48 c, $89 c, $df c,        \ mov rdi, rbx (dst from NOS)
  $48 c, $89 c, $c1 c,        \ mov rcx, rax (count from TOS)
  $f3 c, $a4 c,               \ rep movsb
  \ After rep movsb, rax/rbx/rcx are clobbered. Reload from memory.
  -3 sdepth+!
  stack-depth@ 1 >= if 0 stkptr tos mov-rm  8 stkptr add-ri8 then
  stack-depth@ 2 >= if 0 stkptr nos mov-rm  8 stkptr add-ri8 then
  stack-depth@ 3 >= if 0 stkptr third mov-rm  8 stkptr add-ri8 then ;

: emit-/string ( -- )
  \ ( addr u n -- addr+n u-n )
  \ TOS=n (rax), NOS=u (rbx), 3rd=addr (rcx)
  \ Result: TOS=u-n, NOS=addr+n
  tos third add-rr          \ addr = addr + n (rcx = rcx + rax)
  tos nos sub-rr            \ u = u - n (rbx = rbx - rax)
  pop-val ;                 \ drop n, TOS=u-n, NOS=addr+n

\ ============================================================
\ BITWISE
\ ============================================================

: emit-and ( -- )
  \ ( x y -- x&y ) bitwise AND
  nos tos and-rr
  pop-nos-val ;

: emit-or ( -- )
  \ ( x y -- x|y ) bitwise OR
  nos tos or-rr
  pop-nos-val ;

: emit-xor ( -- )
  \ ( x y -- x^y ) bitwise XOR
  nos tos xor-rr
  pop-nos-val ;

: emit-invert ( -- )
  \ ( x -- ~x ) bitwise NOT
  tos not-r ;

: emit-mod ( -- )
  \ ( n d -- n mod d )
  \ Divide rax by rbx, result remainder in rax
  $48 c, $99 c,                 \ cqo (sign extend rax to rdx:rax)
  $48 c, $f7 c, $fb c,          \ idiv rbx
  $48 c, $89 c, $d0 c,          \ mov rax, rdx (remainder)
  pop-nos-val ;                 \ drop divisor

: emit-/mod ( -- )
  \ ( n d -- rem quot )
  \ Divide rax by rbx, remainder in rdx, quotient in rax
  $48 c, $99 c,                 \ cqo
  $48 c, $f7 c, $fb c,          \ idiv rbx
  \ Now: rax=quot, rdx=rem, need (rem quot) on stack
  \ TOS=quot (rax), NOS should be rem
  $48 c, $89 c, $d3 c, ;        \ mov rbx, rdx (rem to NOS)

: emit-/ ( -- )
  \ ( n d -- n/d ) quotient only
  \ Divide rax by rbx, quotient stays in rax
  $48 c, $99 c,                 \ cqo (sign extend rax to rdx:rax)
  $48 c, $f7 c, $fb c,          \ idiv rbx (quot in rax, rem in rdx)
  pop-nos-val ;                 \ drop divisor, keep quotient in rax

\ ============================================================
\ SHIFT OPERATIONS (runtime, non-immediate)
\ ============================================================

: emit-lshift ( -- )
  \ ( x n -- x<<n ) left shift
  \ n in rax (TOS), x in rbx (NOS)
  \ Need: shl rbx, cl (shift amount in cl)
  $48 c, $89 c, $c1 c,          \ mov rcx, rax
  nos shl-cl                     \ shl rbx, cl
  nos tos mov-rr                 \ mov rax, rbx
  pop-nos-val ;

: emit-rshift ( -- )
  \ ( x n -- x>>n ) logical right shift
  \ n in rax (TOS), x in rbx (NOS)
  $48 c, $89 c, $c1 c,          \ mov rcx, rax
  nos shr-cl                     \ shr rbx, cl
  nos tos mov-rr                 \ mov rax, rbx
  pop-nos-val ;

\ ============================================================
\ ADDITIONAL COMPARISONS
\ ============================================================

: emit-<> ( -- )
  \ ( x y -- flag ) -1 if not equal, 0 otherwise
  $95 emit-setcc ;              \ SETNZ

: emit-u< ( -- )
  \ ( u1 u2 -- flag ) -1 if NOS < TOS (unsigned)
  $92 emit-setcc ;              \ SETB (below, unsigned)

: emit-0<> ( -- )
  \ ( x -- flag ) -1 if TOS is non-zero
  $95 emit-test-setcc ;         \ SETNZ

: emit-<= ( -- )
  \ ( x y -- flag ) -1 if NOS <= TOS (signed)
  $9e emit-setcc ;              \ SETLE

: emit->= ( -- )
  \ ( x y -- flag ) -1 if NOS >= TOS (signed)
  $9d emit-setcc ;              \ SETGE

\ ============================================================
\ ADDITIONAL STACK OPERATIONS
\ ============================================================

: emit-2swap ( -- )
  \ ( a b c d -- c d a b ) swap top two pairs
  \ Stack: TOS=d, NOS=c, 3rd=b, 4th=a (at [r15])
  \ Want:  TOS=b, NOS=a, 3rd=d, 4th=c
  tos third xchg-rr             \ d <-> b: TOS=b, 3rd=d
  \ Now swap NOS (c) with [r15] (a)
  $48 c, $87 c, $1f c, ;        \ xchg rbx, [r15] - NOS <-> 4th

: emit-2over ( -- )
  \ ( a b c d -- a b c d a b ) copy 3rd/4th to TOS
  \ Stack: TOS=d, NOS=c, 3rd=b, 4th=a (at [r15])
  \ Want to push a then b
  push-val                       \ make room, shifts stack
  0 stkptr tos mov-rm            \ mov rax, [r15] - copy 4th (a) to TOS
  push-val                       \ make room again
  third tos mov-rr ;             \ copy what was 3rd (b, now in rcx) to TOS

: emit--rot ( -- )
  \ ( a b c -- c a b ) reverse of rot
  third nos xchg-rr             \ a <-> b: b a c
  tos third xchg-rr ;           \ c <-> b: c a b

: emit-?dup ( -- )
  \ ( x -- x x | 0 ) dup if non-zero
  \ Generate: test rax,rax / jz skip / mov rbx,rax (dup) / skip:
  \ This is tricky - stack depth changes conditionally
  \ Simplest: test + conditional dup via cmov or branch
  tos tos test-rr               \ test rax, rax
  $74 c, $06 c,                 \ jz skip (+6 bytes to skip the mov rbx,rbx; mov rbx,rax)
  RAX RBX mov-rr                \ mov rbx, rax (3 bytes)
  RBX RCX mov-rr ;              \ mov rcx, rbx (3 bytes) - shuffle down

: emit-depth ( -- )
  \ ( -- n ) push data stack depth
  \ Stack depth = (r14 - rsp) / 8
  push-val                       \ make room
  $48 c, $89 c, $e0 c,          \ mov rax, rsp
  $4c c, $29 c, $f0 c,          \ sub rax, r14 (bottom - current)
  $48 c, $c1 c, $f8 c, $03 c,   \ sar rax, 3 (divide by 8)
  tos neg-r ;                   \ negate (was negative)

: emit-pick ( -- )
  \ ( xn ... x0 n -- xn ... x0 xn ) copy nth item
  \ n is in rax, need to access [rsp + n*8]
  $48 c, $8b c, $04 c, $c4 c, ; \ mov rax, [rsp + rax*8]

\ ============================================================
\ MIN/MAX/ABS
\ ============================================================

: emit-min ( -- )
  \ ( a b -- min ) signed minimum
  tos nos cmp-rr                \ cmp rbx, rax
  $48 c, $0f c, $4c c, $c3 c,   \ cmovl rax, rbx (if rbx < rax, rax = rbx)
  pop-nos-val ;

: emit-max ( -- )
  \ ( a b -- max ) signed maximum
  tos nos cmp-rr                \ cmp rbx, rax
  $48 c, $0f c, $4f c, $c3 c,   \ cmovg rax, rbx (if rbx > rax, rax = rbx)
  pop-nos-val ;

: emit-abs ( -- )
  \ ( n -- |n| ) absolute value
  $48 c, $89 c, $c3 c,          \ mov rbx, rax
  $48 c, $c1 c, $fb c, $3f c,   \ sar rbx, 63 (sign extend to all bits)
  $48 c, $31 c, $d8 c,          \ xor rax, rbx
  $48 c, $29 c, $d8 c, ;        \ sub rax, rbx

: emit-throw ( -- )
  \ ( n -- ) if n != 0, exit with code n
  tos tos test-rr               \ test rax, rax
  $74 c, $09 c,                 \ jz skip (+9 bytes)
  $48 c, $89 c, $c7 c,          \ mov rdi, rax
  $b8 c, 60 d,                  \ mov eax, 60 (exit syscall)
  $0f c, $05 c,                 \ syscall
  \ skip: (fall through, consume the 0)
  pop-val ;

: emit-fill ( -- )
  \ ( addr u char -- ) fill u bytes at addr with char
  \ char=rax(TOS), u=rbx(NOS), addr=rcx(3rd)
  $48 c, $89 c, $cf c,          \ mov rdi, rcx (addr)
  $48 c, $89 c, $d9 c,          \ mov rcx, rbx (count)
  $f3 c, $aa c,                 \ rep stosb (al -> [rdi], rdi++, rcx--)
  pop-val pop-val pop-val ;     \ drop all 3 args

\ ============================================================
\ FILE ACCESS MODE CONSTANTS
\ ============================================================

: emit-w/o ( -- )
  \ ( -- fam ) push O_WRONLY|O_CREAT|O_TRUNC = 577
  push-val
  $b8 c, 577 d, ;               \ mov eax, 577

: emit-r/o ( -- )
  \ ( -- fam ) push O_RDONLY = 0
  push-val
  $b8 c, 0 d, ;                 \ mov eax, 0

: emit-r/w ( -- )
  \ ( -- fam ) push O_RDWR = 2
  push-val
  $b8 c, 2 d, ;                 \ mov eax, 2

: emit-bye ( -- )
  \ Exit program with code 0
  $b8 c, 60 d,                  \ mov eax, 60 (exit syscall)
  $31 c, $ff c,                 \ xor edi, edi (exit code 0)
  $0f c, $05 c, ;               \ syscall

: emit-write-file ( -- )
  \ ( addr u fd -- ior )
  \ fd=rax(TOS), u=rbx(NOS), addr=rcx(3rd)
  $48 c, $89 c, $c7 c,          \ mov rdi, rax (fd)
  $48 c, $89 c, $ce c,          \ mov rsi, rcx (addr)
  $48 c, $89 c, $da c,          \ mov rdx, rbx (count)
  $b8 c, 1 d,                   \ mov eax, 1 (sys_write)
  $0f c, $05 c,                 \ syscall
  \ rax = bytes written (>=0) or -errno (<0)
  \ Return 0 for success, -1 for error
  $48 c, $85 c, $c0 c,          \ test rax, rax
  $78 c, 5 c,                   \ js +5 (jump if error)
  $48 c, $31 c, $c0 c,          \ xor eax, eax (success = 0)
  $eb c, 7 c,                   \ jmp +7 (skip error path)
  $48 c, $c7 c, $c0 c, -1 d,    \ mov rax, -1 (error)
  pop-nos-val pop-nos-val ;     \ drop u and addr, keep ior in rax

: emit-close-file ( -- )
  \ ( fd -- ior )
  \ fd=rax(TOS)
  $48 c, $89 c, $c7 c,          \ mov rdi, rax (fd)
  $b8 c, 3 d,                   \ mov eax, 3 (sys_close)
  $0f c, $05 c,                 \ syscall
  \ rax = 0 on success, -errno on failure
  $48 c, $85 c, $c0 c,          \ test rax, rax
  $74 c, 7 c,                   \ jz +7 (skip if success)
  $48 c, $c7 c, $c0 c, -1 d, ;  \ mov rax, -1

\ ============================================================
\ ARGC/ARGV
\ ============================================================

: emit-argc ( -- )
  \ ( -- n ) push argument count
  push-val
  $48 c, $8b c, $04 c, $25 c, rt-argc d, ;   \ mov rax, [rt-argc]

: emit-argv ( -- )
  \ ( n -- addr u ) get nth argument string
  \ Input: n in rax (TOS)
  \ Output: addr in rbx (NOS), len in rax (TOS)
  $48 c, $8b c, $3c c, $25 c, rt-argv d,   \ mov rdi, [rt-argv]
  $48 c, $8b c, $3c c, $c7 c,              \ mov rdi, [rdi+rax*8] = argv[n]
  \ rdi = pointer to null-terminated string, find length
  $48 c, $31 c, $c0 c,                     \ xor rax, rax (length counter)
  code-here                                 \ loop:
  $80 c, $3c c, $07 c, 0 c,                \ cmp byte [rdi+rax], 0
  $74 c, 5 c,                              \ jz done
  $48 c, $ff c, $c0 c,                     \ inc rax
  $eb c, code-here - 1- negate c,          \ jmp loop
  \ rax = length, rdi = start address
  \ Result: NOS=addr, TOS=len
  push-val                                  \ make room for addr
  $48 c, $89 c, $fb c, ;                   \ mov rbx, rdi (addr to NOS)
