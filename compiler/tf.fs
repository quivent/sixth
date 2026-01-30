\ tf.fs - Fifth Native Compiler
\ Self-hosting: fifth tf.fs tf.fs → native tf compiler
\ Then: ./tf program.fs → native program

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

\ Compilation state
variable state  0 state !   \ 0=interpret, 1=compile
variable tos-cached  1 tos-cached !  \ Track if TOS is in rax

\ ============================================================
\ CODE EMISSION
\ ============================================================

: c, ( b -- ) code-buf code-pos @ + c!  1 code-pos +! ;
: d, ( d -- ) dup c, 8 rshift dup c, 8 rshift dup c, 8 rshift c, ;
: q, ( q -- ) dup d, 32 rshift d, ;

: code-here ( -- addr ) code-pos @ ;
: patch-rel32 ( target from -- )
  \ Calculate rel32 offset and store as 4 bytes (not cell!)
  dup >r  4 + -               \ offset = target - (from + 4)
  r> code-buf +               \ addr = code-buf + from
  2dup c!                     \ byte 0
  swap 8 rshift swap 1+ 2dup c!   \ byte 1
  swap 8 rshift swap 1+ 2dup c!   \ byte 2
  swap 8 rshift swap 1+ c! ;      \ byte 3

\ ============================================================
\ ELF EMISSION
\ ============================================================

: e, ( b -- ) elf-buf elf-pos @ + c!  1 elf-pos +! ;
: e2, ( w -- ) dup e, 8 rshift e, ;
: e4, ( d -- ) dup e2, 16 rshift e2, ;
: e8, ( q -- ) dup e4, 32 rshift e4, ;

: elf-header ( -- )
  0 elf-pos !
  $7f e, 69 e, 76 e, 70 e,       \ ELF magic
  2 e, 1 e, 1 e, 0 e,             \ 64-bit, little-endian, ELF v1, SysV ABI
  0 e8,                           \ padding
  2 e2, $3e e2, 1 e4,             \ executable, x86_64, ELF v1
  $400078 e8,                     \ entry point
  64 e8, 0 e8, 0 e4,              \ phoff=64, shoff=0, flags=0
  64 e2, 56 e2, 1 e2,             \ ehsize=64, phentsize=56, phnum=1
  0 e2, 0 e2, 0 e2,               \ shentsize=0, shnum=0, shstrndx=0
  \ Program header
  1 e4, 7 e4, 0 e8,               \ PT_LOAD, flags=RWX, offset=0
  $400000 e8, $400000 e8,         \ vaddr, paddr
  120 code-pos @ + e8,            \ filesz = header + code
  $10000 e8,                      \ memsz = 64KB (includes data stack at 0x408000)
  $1000 e8, ;                     \ align

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
: dict-name ( entry -- addr ) ;  \ first 24 bytes
: dict-addr ( entry -- addr ) 24 + ;
: dict-flags ( entry -- addr ) 28 + ;

: dict-add ( addr u -- )
  dict-entry >r
  r@ 24 0 fill          \ clear name field
  dup 23 > if drop 23 then  \ truncate
  r@ swap move          \ copy name
  code-here r@ dict-addr !   \ store code address
  0 r> dict-flags !     \ clear flags
  1 dict-count +! ;

: dict-name= ( addr u entry -- flag )
  \ Compare name against null-padded 24-byte field
  >r
  dup 23 > if 2drop r> drop false exit then
  r@ over + c@ 0 <> if r> drop 2drop false exit then  \ check byte after name is 0
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

\ Push rax to stack: sub r15,8; mov [r15],rax
: push-tos ( -- )
  $49 c, $83 c, $ef c, 8 c,      \ sub r15, 8
  $49 c, $89 c, $07 c, ;         \ mov [r15], rax

\ Pop to rax: mov rax,[r15]; add r15,8
: pop-tos ( -- )
  $49 c, $8b c, $07 c,           \ mov rax, [r15]
  $49 c, $83 c, $c7 c, 8 c, ;    \ add r15, 8

\ ============================================================
\ CODE GENERATION - PRIMITIVES
\ ============================================================

: gen-lit ( n -- )
  push-tos
  dup 0= if
    drop
    $31 c, $c0 c,                 \ xor eax, eax (2 bytes)
  else dup $7FFFFFFF <= over $FFFFFFFF80000000 >= and if
    $48 c, $c7 c, $c0 c, d,       \ mov rax, imm32 (sign-extended, 7 bytes)
  else
    $48 c, $b8 c, q,              \ mov rax, imm64 (10 bytes)
  then then ;

: gen-dup ( -- )
  push-tos ;

: gen-drop ( -- )
  pop-tos ;

: gen-swap ( -- )
  \ xchg rax, [r15]
  $49 c, $87 c, $07 c, ;

: gen-over ( -- )
  push-tos
  $49 c, $8b c, $47 c, 8 c, ;    \ mov rax, [r15+8]

: gen-rot ( -- )
  \ a b c -- b c a  (stack: [r15+8]=a [r15]=b, rax=c)
  \ Want: [r15+8]=b [r15]=c rax=a
  $49 c, $8b c, $4f c, 8 c,      \ mov rcx, [r15+8]   ; a
  $49 c, $8b c, $1f c,           \ mov rbx, [r15]     ; b
  $49 c, $89 c, $5f c, 8 c,      \ mov [r15+8], rbx   ; store b
  $49 c, $89 c, $07 c,           \ mov [r15], rax     ; store c
  $48 c, $89 c, $c8 c, ;         \ mov rax, rcx       ; a to TOS

: gen-nip ( -- )  \ a b -- b
  $49 c, $83 c, $c7 c, 8 c, ;    \ add r15, 8

: gen-tuck ( -- )  \ a b -- b a b
  $49 c, $8b c, $1f c,           \ mov rbx, [r15]     ; a
  $49 c, $89 c, $07 c,           \ mov [r15], rax     ; b
  $49 c, $83 c, $ef c, 8 c,      \ sub r15, 8
  $49 c, $89 c, $1f c, ;         \ mov [r15], rbx     ; a

: gen-2dup ( -- )
  \ ( a b -- a b a b ) with TOS caching: rax=b, [r15]=a
  \ Result: rax=b, [r15]=a, [r15+8]=b, [r15+16]=a (original)
  $49 c, $8b c, $1f c,           \ mov rbx, [r15]     ; rbx = a
  $49 c, $83 c, $ef c, 16 c,     \ sub r15, 16
  $49 c, $89 c, $47 c, 8 c,      \ mov [r15+8], rax   ; store b copy
  $49 c, $89 c, $1f c, ;         \ mov [r15], rbx     ; store a copy

: gen-2drop ( -- )
  $49 c, $8b c, $47 c, 8 c,      \ mov rax, [r15+8]
  $49 c, $83 c, $c7 c, 16 c, ;   \ add r15, 16

\ ============================================================
\ ARITHMETIC
\ ============================================================

: gen-add ( -- )
  $49 c, $03 c, $07 c,           \ add rax, [r15]
  $49 c, $83 c, $c7 c, 8 c, ;    \ add r15, 8

: gen-sub ( -- )
  $49 c, $8b c, $1f c,           \ mov rbx, [r15]
  $49 c, $83 c, $c7 c, 8 c,      \ add r15, 8
  $48 c, $29 c, $c3 c,           \ sub rbx, rax
  $48 c, $89 c, $d8 c, ;         \ mov rax, rbx

: gen-mul ( -- )
  $49 c, $0f c, $af c, $07 c,    \ imul rax, [r15]
  $49 c, $83 c, $c7 c, 8 c, ;    \ add r15, 8

: gen-div ( -- )
  $49 c, $8b c, $1f c,           \ mov rbx, [r15]
  $49 c, $83 c, $c7 c, 8 c,      \ add r15, 8
  $48 c, $92 c,                  \ xchg rax, rdx
  $48 c, $89 c, $d8 c,           \ mov rax, rbx
  $48 c, $99 c,                  \ cqo
  $49 c, $8b c, $1f c,           \ mov rbx, [r15]... wait, already popped
  \ Redo: a/b where b is TOS
  ;

\ Simpler div: ( a b -- a/b )
: gen-div ( -- )
  $48 c, $89 c, $c1 c,           \ mov rcx, rax       ; divisor
  $49 c, $8b c, $07 c,           \ mov rax, [r15]     ; dividend
  $49 c, $83 c, $c7 c, 8 c,      \ add r15, 8
  $48 c, $99 c,                  \ cqo
  $48 c, $f7 c, $f9 c, ;         \ idiv rcx

: gen-mod ( -- )
  $48 c, $89 c, $c1 c,           \ mov rcx, rax
  $49 c, $8b c, $07 c,           \ mov rax, [r15]
  $49 c, $83 c, $c7 c, 8 c,      \ add r15, 8
  $48 c, $99 c,                  \ cqo
  $48 c, $f7 c, $f9 c,           \ idiv rcx
  $48 c, $89 c, $d0 c, ;         \ mov rax, rdx

: gen-negate ( -- )
  $48 c, $f7 c, $d8 c, ;         \ neg rax

: gen-1+ ( -- )
  $48 c, $ff c, $c0 c, ;         \ inc rax

: gen-1- ( -- )
  $48 c, $ff c, $c8 c, ;         \ dec rax

: gen-and ( -- )
  $49 c, $23 c, $07 c,           \ and rax, [r15]
  $49 c, $83 c, $c7 c, 8 c, ;    \ add r15, 8

: gen-or ( -- )
  $49 c, $0b c, $07 c,           \ or rax, [r15]
  $49 c, $83 c, $c7 c, 8 c, ;    \ add r15, 8

: gen-xor ( -- )
  $49 c, $33 c, $07 c,           \ xor rax, [r15]
  $49 c, $83 c, $c7 c, 8 c, ;    \ add r15, 8

: gen-invert ( -- )
  $48 c, $f7 c, $d0 c, ;         \ not rax

\ ============================================================
\ COMPARISON (result: 0 or -1)
\ ============================================================

: gen-cmp-setup ( -- )
  $49 c, $8b c, $1f c,           \ mov rbx, [r15]
  $49 c, $83 c, $c7 c, 8 c,      \ add r15, 8
  $48 c, $39 c, $c3 c, ;         \ cmp rbx, rax

: gen-= ( -- )
  gen-cmp-setup
  $0f c, $94 c, $c0 c,           \ sete al
  $48 c, $0f c, $b6 c, $c0 c,    \ movzx rax, al
  $48 c, $f7 c, $d8 c, ;         \ neg rax (0->0, 1->-1)

: gen-<> ( -- )
  gen-cmp-setup
  $0f c, $95 c, $c0 c,           \ setne al
  $48 c, $0f c, $b6 c, $c0 c,
  $48 c, $f7 c, $d8 c, ;

: gen-< ( -- )
  gen-cmp-setup
  $0f c, $9c c, $c0 c,           \ setl al
  $48 c, $0f c, $b6 c, $c0 c,
  $48 c, $f7 c, $d8 c, ;

: gen-> ( -- )
  gen-cmp-setup
  $0f c, $9f c, $c0 c,           \ setg al
  $48 c, $0f c, $b6 c, $c0 c,
  $48 c, $f7 c, $d8 c, ;

: gen-<= ( -- )
  gen-cmp-setup
  $0f c, $9e c, $c0 c,           \ setle al
  $48 c, $0f c, $b6 c, $c0 c,
  $48 c, $f7 c, $d8 c, ;

: gen->= ( -- )
  gen-cmp-setup
  $0f c, $9d c, $c0 c,           \ setge al
  $48 c, $0f c, $b6 c, $c0 c,
  $48 c, $f7 c, $d8 c, ;

: gen-0= ( -- )
  $48 c, $85 c, $c0 c,           \ test rax, rax
  $0f c, $94 c, $c0 c,           \ sete al
  $48 c, $0f c, $b6 c, $c0 c,
  $48 c, $f7 c, $d8 c, ;

: gen-0< ( -- )
  $48 c, $c1 c, $f8 c, 63 c,     \ sar rax, 63
  ;

\ ============================================================
\ MEMORY
\ ============================================================

: gen-@ ( -- )
  $48 c, $8b c, $00 c, ;         \ mov rax, [rax]

: gen-! ( -- )
  $49 c, $8b c, $1f c,           \ mov rbx, [r15]
  $48 c, $89 c, $18 c,           \ mov [rax], rbx
  $49 c, $8b c, $47 c, 8 c,      \ mov rax, [r15+8]
  $49 c, $83 c, $c7 c, 16 c, ;   \ add r15, 16

: gen-c@ ( -- )
  $48 c, $0f c, $b6 c, $00 c, ;  \ movzx rax, byte [rax]

: gen-c! ( -- )
  $49 c, $8b c, $1f c,           \ mov rbx, [r15]
  $88 c, $18 c,                  \ mov [rax], bl
  $49 c, $8b c, $47 c, 8 c,      \ mov rax, [r15+8]
  $49 c, $83 c, $c7 c, 16 c, ;   \ add r15, 16

\ ============================================================
\ CONTROL FLOW
\ ============================================================

: gen-if ( -- orig )
  \ TOS is condition; test and consume it, branch if zero
  \ Save condition result before pop (pop clobbers flags)
  $48 c, $85 c, $c0 c,           \ test rax, rax
  $0f c, $94 c, $c1 c,           \ setz cl (save result)
  pop-tos                        \ load new TOS (consumes condition)
  $84 c, $c9 c,                  \ test cl, cl
  $0f c, $85 c,                  \ jnz rel32 (jump if was zero)
  0 d,                           \ placeholder
  code-here ;                    \ leave address for patching (after rel32)

\ Optimized compare+branch: skip flag conversion, use CPU flags directly
\ These consume BOTH TOS and NOS (like < if combined)
: gen-<if ( -- orig )
  \ ( a b -- ) branch if NOT a<b (i.e., if a>=b)
  $49 c, $8b c, $1f c,           \ mov rbx, [r15] (NOS = a)
  $48 c, $39 c, $c3 c,           \ cmp rbx, rax (cmp a, b)
  $49 c, $8b c, $47 c, 8 c,      \ mov rax, [r15+8] (new TOS from under both)
  $4d c, $8d c, $7f c, 16 c,     \ lea r15, [r15+16] (pop both) - no flag change!
  $0f c, $8d c,                  \ jge rel32 (jump if a >= b)
  0 d,
  code-here ;

: gen->if ( -- orig )
  \ ( a b -- ) branch if NOT a>b (i.e., if a<=b)
  $49 c, $8b c, $1f c,           \ mov rbx, [r15]
  $48 c, $39 c, $c3 c,           \ cmp rbx, rax
  $49 c, $8b c, $47 c, 8 c,      \ mov rax, [r15+8]
  $4d c, $8d c, $7f c, 16 c,     \ lea r15, [r15+16]
  $0f c, $8e c,                  \ jle rel32
  0 d,
  code-here ;

: gen-=if ( -- orig )
  \ ( a b -- ) branch if NOT a=b (i.e., if a<>b)
  $49 c, $8b c, $1f c,           \ mov rbx, [r15]
  $48 c, $39 c, $c3 c,           \ cmp rbx, rax
  $49 c, $8b c, $47 c, 8 c,      \ mov rax, [r15+8]
  $4d c, $8d c, $7f c, 16 c,     \ lea r15, [r15+16]
  $0f c, $85 c,                  \ jne rel32
  0 d,
  code-here ;

: gen-0<if ( -- orig )
  \ ( n -- ) branch if NOT n<0 (i.e., if n>=0)
  $48 c, $85 c, $c0 c,           \ test rax, rax
  pop-tos
  $0f c, $89 c,                  \ jns rel32 (jump if NOT negative)
  0 d,
  code-here ;

: gen-0=if ( -- orig )
  \ ( n -- ) branch if NOT n=0 (i.e., if n<>0)
  $48 c, $85 c, $c0 c,           \ test rax, rax
  pop-tos
  $0f c, $85 c,                  \ jne rel32 (jump if NOT zero)
  0 d,
  code-here ;

: gen-else ( orig1 -- orig2 )
  $e9 c,                         \ jmp rel32
  code-here 0 d,
  code-here swap                      \ ( new-orig old-orig )
  code-here swap 4 - patch-rel32 ;    \ patch old jump to code-here

: gen-then ( orig -- )
  code-here swap 4 - patch-rel32 ;    \ patch jump to code-here

: gen-begin ( -- dest )
  code-here ;

: gen-until ( dest -- )
  \ Save condition, pop new TOS, then test (pop clobbers flags)
  $48 c, $89 c, $c1 c,           \ mov rcx, rax (save condition)
  pop-tos                         \ get new TOS into rax
  $48 c, $85 c, $c9 c,           \ test rcx, rcx
  $0f c, $84 c,                  \ jz rel32
  code-here 4 + - d, ;

: gen-0=until ( dest -- )
  \ Tight loop: exit when TOS=0, loop when TOS!=0
  \ Replaces "dup 0= until" pattern - 5 ops instead of 12
  $48 c, $85 c, $c0 c,           \ test rax, rax
  $0f c, $95 c, $c1 c,           \ setnz cl (cl=1 if non-zero, keep looping)
  pop-tos                         \ rax = new TOS
  $84 c, $c9 c,                  \ test cl, cl
  $0f c, $85 c,                  \ jnz rel32 (loop back if was non-zero)
  code-here 4 + - d, ;

: gen-nzloop ( dest -- )
  \ Tightest loop: test TOS, loop if non-zero, keep TOS
  \ Note: if preceded by 1- (dec), flags already set - but we test anyway
  $48 c, $85 c, $c0 c,           \ test rax, rax
  $0f c, $85 c,                  \ jnz rel32
  code-here 4 + - d, ;

: gen-1-nzloop ( dest -- )
  \ Peephole: 1- nzloop as single 2-instruction sequence
  \ dec sets ZF, no test needed
  $48 c, $ff c, $c8 c,           \ dec rax
  $0f c, $85 c,                  \ jnz rel32
  code-here 4 + - d, ;

: gen-while ( dest -- orig dest )
  \ Save condition, pop new TOS, then test (pop clobbers flags)
  $48 c, $89 c, $c1 c,           \ mov rcx, rax (save condition)
  pop-tos                         \ get new TOS into rax
  $48 c, $85 c, $c9 c,           \ test rcx, rcx
  $0f c, $84 c,                  \ jz rel32
  0 d,
  code-here swap ;

: gen-repeat ( orig dest -- )
  $e9 c,                         \ jmp rel32 (backward to dest)
  code-here 4 + - d,
  code-here swap 4 - patch-rel32 ;    \ patch forward jump to code-here

: gen-again ( dest -- )
  $e9 c,                         \ jmp rel32
  code-here 4 + - d, ;

\ ============================================================
\ CALLS
\ ============================================================

: gen-call ( addr -- )
  $e8 c,                         \ call rel32
  code-here 4 + - d, ;

: gen-ret ( -- )
  $c3 c, ;

\ ============================================================
\ DO/LOOP (uses x86 stack as return stack)
\ ============================================================

\ do ( limit index -- )  R: -- limit index
\ TOS=index, [r15]=limit
: gen-do ( -- do-addr )
  $49 c, $8b c, $1f c,           \ mov rbx, [r15]  ; limit
  $49 c, $83 c, $c7 c, 8 c,      \ add r15, 8      ; drop limit from data stack
  $53 c,                         \ push rbx        ; limit to return stack
  $50 c,                         \ push rax        ; index to return stack
  pop-tos                        \ new TOS from data stack
  code-here ;                         \ leave loop start address

\ loop ( -- )  R: limit index -- | limit index+1
: gen-loop ( do-addr -- )
  $58 c,                         \ pop rax         ; index
  $48 c, $ff c, $c0 c,           \ inc rax
  $48 c, $3b c, $04 c, $24 c,    \ cmp rax, [rsp]  ; compare with limit
  $7c c, 3 c,                    \ jl +3           ; if less, continue loop
  $58 c,                         \ pop (discard limit)
  $eb c, 0 c,                    \ jmp exit (patched below)
  code-here swap                      \ ( exit-patch do-addr )
  $50 c,                         \ push rax        ; save index
  $e9 c,                         \ jmp do-addr
  code-here 4 + - d,                  \ backward jump; consumes do-addr, leaves ( exit-patch )
  code-here over -                    \ offset = code-here - exit-patch: ( exit-patch offset )
  swap 1- code-buf + c! ;        \ store at displacement byte

\ +loop ( n -- )  R: limit index -- | limit index+n
: gen-+loop ( do-addr -- )
  $5b c,                         \ pop rbx         ; index
  $48 c, $01 c, $c3 c,           \ add rbx, rax    ; index += n
  pop-tos                        \ get new TOS
  $48 c, $3b c, $1c c, $24 c,    \ cmp rbx, [rsp]  ; compare with limit
  $7c c, 3 c,                    \ jl +3           ; if less, continue
  $58 c,                         \ pop (discard limit)
  $eb c, 0 c,                    \ jmp exit (patched below)
  code-here swap                      \ ( exit-patch do-addr )
  $53 c,                         \ push rbx        ; save index
  $e9 c,                         \ jmp do-addr
  code-here 4 + - d,                  \ backward jump; consumes do-addr, leaves ( exit-patch )
  code-here over -                    \ offset = code-here - exit-patch
  swap 1- code-buf + c! ;        \ store at displacement byte

\ i ( -- index )  copy loop index to data stack
: gen-i ( -- )
  push-tos
  $48 c, $8b c, $04 c, $24 c, ;  \ mov rax, [rsp]

\ j ( -- index )  outer loop index
: gen-j ( -- )
  push-tos
  $48 c, $8b c, $44 c, $24 c, 16 c, ;  \ mov rax, [rsp+16]

\ ============================================================
\ RECURSE
\ ============================================================

variable current-word-addr  0 current-word-addr !

: gen-recurse ( -- )
  current-word-addr @ gen-call ;

\ ============================================================
\ PROLOGUE / EPILOGUE
\ ============================================================

variable start-jmp  \ Address of jump-to-start placeholder

: gen-prologue ( -- )
  \ Set up r15 as data stack (use area after code)
  $49 c, $bf c, $400000 $8000 + q,  \ mov r15, 0x408000 (32KB into segment)
  \ Jump forward to main call (patched later)
  $e9 c, code-here start-jmp ! 0 d,      \ jmp rel32 (placeholder)
  ;

: patch-start ( -- )
  \ Patch the start jump to current position
  code-here start-jmp @ patch-rel32 ;

: gen-epilogue ( -- )
  \ Exit syscall
  $b8 c, 60 d,                   \ mov eax, 60
  $31 c, $ff c,                  \ xor edi, edi
  $0f c, $05 c, ;                \ syscall

\ ============================================================
\ PRINT NUMBER (for . word)
\ ============================================================

: gen-dot ( -- )
  $b9 c, 10 d,                   \ mov ecx, 10
  $45 c, $31 c, $c0 c,           \ xor r8d, r8d
  \ Check for negative
  $48 c, $85 c, $c0 c,           \ test rax, rax
  $79 c, 28 c,                   \ jns positive (+28 to skip neg handling)
  $50 c,                         \ push rax
  $b8 c, 1 d,                    \ mov eax, 1
  $bf c, 1 d,                    \ mov edi, 1
  $6a c, 45 c,                   \ push '-'
  $48 c, $89 c, $e6 c,           \ mov rsi, rsp
  $ba c, 1 d,                    \ mov edx, 1
  $0f c, $05 c,                  \ syscall
  $58 c,                         \ pop (minus sign)
  $58 c,                         \ pop rax
  $48 c, $f7 c, $d8 c,           \ neg rax
  \ digit_loop:
  $31 c, $d2 c,                  \ xor edx, edx
  $f7 c, $f1 c,                  \ div ecx
  $83 c, $c2 c, $30 c,           \ add edx, '0'
  $52 c,                         \ push rdx
  $41 c, $ff c, $c0 c,           \ inc r8d
  $85 c, $c0 c,                  \ test eax, eax
  $75 c, $f1 c,                  \ jnz digit_loop
  \ print_loop:
  $b8 c, 1 d,                    \ mov eax, 1
  $bf c, 1 d,                    \ mov edi, 1
  $48 c, $89 c, $e6 c,           \ mov rsi, rsp
  $ba c, 1 d,                    \ mov edx, 1
  $0f c, $05 c,                  \ syscall
  $58 c,                         \ pop
  $41 c, $ff c, $c8 c,           \ dec r8d
  $75 c, $e6 c,                  \ jnz print_loop
  \ space
  $6a c, 32 c,                   \ push ' '
  $b8 c, 1 d,
  $bf c, 1 d,
  $48 c, $89 c, $e6 c,
  $ba c, 1 d,
  $0f c, $05 c,
  $58 c,
  pop-tos ;

: gen-cr ( -- )
  $50 c,                         \ push rax  (save TOS)
  $6a c, 10 c,                   \ push 10
  $b8 c, 1 d,                    \ mov eax, 1  (write)
  $bf c, 1 d,                    \ mov edi, 1  (stdout)
  $48 c, $89 c, $e6 c,           \ mov rsi, rsp
  $ba c, 1 d,                    \ mov edx, 1
  $0f c, $05 c,                  \ syscall
  $58 c,                         \ pop (newline)
  $58 c, ;                       \ pop rax  (restore TOS)

: gen-emit ( -- )
  push-tos                       \ save for later pop
  $50 c,                         \ push rax (char)
  $b8 c, 1 d,                    \ mov eax, 1
  $bf c, 1 d,                    \ mov edi, 1
  $48 c, $89 c, $e6 c,           \ mov rsi, rsp
  $ba c, 1 d,                    \ mov edx, 1
  $0f c, $05 c,                  \ syscall
  $58 c,                         \ pop
  pop-tos                        \ new TOS
  pop-tos ;                      \ consume the emitted char

\ ============================================================
\ STRING COMPARE
\ ============================================================

\ Static string storage. We allocate space for comparison strings.
\ Format: length byte followed by characters
here constant static-strings-start
\ Leave 2KB for static strings
2048 allot
here constant static-strings-end
static-strings-start constant str-ptr
variable str-ptr-cur  static-strings-start str-ptr-cur !

\ Store a string permanently in static area, return addr u
: s, ( addr u -- static-addr u )
  dup str-ptr-cur @ + static-strings-end < invert if
    ." Static string overflow" cr 1 throw
  then
  \ Stack: ( src-addr u )
  dup >r                     \ save u
  str-ptr-cur @ swap         \ ( src-addr dest-addr u )
  move                       \ copy u bytes from src to dest
  str-ptr-cur @ r>           \ ( dest-addr u )
  dup str-ptr-cur +! ;       \ advance str-ptr-cur by u

\ Compare strings (addr1 must be stable, addr2 can be transient)
: str= ( addr1 u1 addr2 u2 -- flag )
  rot over <> if 2drop drop false exit then
  ( addr1 addr2 u )
  begin
    dup 0= if drop 2drop true exit then
    >r over c@ over c@ <> if 2drop r> drop false exit then
    1+ swap 1+ swap r> 1-
  again ;

\ Create all builtin word names as static strings
s" dup" s, 2constant $dup
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
s" and" s, 2constant $and
s" or" s, 2constant $or
s" xor" s, 2constant $xor
s" invert" s, 2constant $invert
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
s" @" s, 2constant $@
s" !" s, 2constant $!
s" c@" s, 2constant $c@
s" c!" s, 2constant $c!
s" ." s, 2constant $.
s" cr" s, 2constant $cr
s" emit" s, 2constant $emit
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
  \ Check for comment
  input-buf input-pos @ + c@ [char] \ = if
    skip-line
    recurse exit
  then
  \ Check for ( comment
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
  \ Check for hex $xx
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
  \ Decimal
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
  \ Can't store execution tokens this way - need different approach
  ;

\ Alternative: check each word directly using static strings
: compile-builtin ( addr u -- found? )
  2dup $dup str= if 2drop gen-dup true exit then
  2dup $drop str= if 2drop gen-drop true exit then
  2dup $swap str= if 2drop gen-swap true exit then
  2dup $over str= if 2drop gen-over true exit then
  2dup $rot str= if 2drop gen-rot true exit then
  2dup $nip str= if 2drop gen-nip true exit then
  2dup $tuck str= if 2drop gen-tuck true exit then
  2dup $2dup str= if 2drop gen-2dup true exit then
  2dup $2drop str= if 2drop gen-2drop true exit then
  2dup $+ str= if 2drop gen-add true exit then
  2dup $- str= if 2drop gen-sub true exit then
  2dup $* str= if 2drop gen-mul true exit then
  2dup $/ str= if 2drop gen-div true exit then
  2dup $mod str= if 2drop gen-mod true exit then
  2dup $negate str= if 2drop gen-negate true exit then
  2dup $1+ str= if 2drop gen-1+ true exit then
  2dup $1- str= if 2drop gen-1- true exit then
  2dup $and str= if 2drop gen-and true exit then
  2dup $or str= if 2drop gen-or true exit then
  2dup $xor str= if 2drop gen-xor true exit then
  2dup $invert str= if 2drop gen-invert true exit then
  2dup $= str= if 2drop gen-= true exit then
  2dup $<> str= if 2drop gen-<> true exit then
  2dup $< str= if 2drop gen-< true exit then
  2dup $> str= if 2drop gen-> true exit then
  2dup $<= str= if 2drop gen-<= true exit then
  2dup $>= str= if 2drop gen->= true exit then
  2dup $0= str= if 2drop gen-0= true exit then
  2dup $0< str= if 2drop gen-0< true exit then
  2dup $@ str= if 2drop gen-@ true exit then
  2dup $! str= if 2drop gen-! true exit then
  2dup $c@ str= if 2drop gen-c@ true exit then
  2dup $c! str= if 2drop gen-c! true exit then
  2dup $. str= if 2drop gen-dot true exit then
  2dup $cr str= if 2drop gen-cr true exit then
  2dup $emit str= if 2drop gen-emit true exit then
  2dup $if str= if 2drop gen-if cf-push true exit then
  2dup $<if str= if 2drop gen-<if cf-push true exit then
  2dup $>if str= if 2drop gen->if cf-push true exit then
  2dup $=if str= if 2drop gen-=if cf-push true exit then
  2dup $0<if str= if 2drop gen-0<if cf-push true exit then
  2dup $0=if str= if 2drop gen-0=if cf-push true exit then
  2dup $else str= if 2drop cf-pop gen-else cf-push true exit then
  2dup $then str= if 2drop cf-pop gen-then true exit then
  2dup $begin str= if 2drop gen-begin cf-push true exit then
  2dup $until str= if 2drop cf-pop gen-until true exit then
  2dup $0=until str= if 2drop cf-pop gen-0=until true exit then
  2dup $nzloop str= if 2drop cf-pop gen-nzloop true exit then
  2dup $1-nzloop str= if 2drop cf-pop gen-1-nzloop true exit then
  2dup $while str= if 2drop cf-pop gen-while cf-push cf-push true exit then
  2dup $repeat str= if 2drop cf-pop cf-pop gen-repeat true exit then
  2dup $again str= if 2drop cf-pop gen-again true exit then
  2dup $do str= if 2drop gen-do cf-push true exit then
  2dup $loop str= if 2drop cf-pop gen-loop true exit then
  2dup $+loop str= if 2drop cf-pop gen-+loop true exit then
  2dup $i str= if 2drop gen-i true exit then
  2dup $j str= if 2drop gen-j true exit then
  2dup $recurse str= if 2drop gen-recurse true exit then
  2dup $exit str= if 2drop gen-ret true exit then
  2drop false ;

\ ============================================================
\ COMPILER
\ ============================================================

variable current-def  0 current-def !

: compile-token ( addr u -- )
  \ Try as builtin
  2dup compile-builtin if 2drop exit then
  \ Try as number
  2dup parse-number if
    \ Stack: addr u n (after if consumed true)
    nip nip     \ n
    gen-lit exit
  then
  \ Try as user word
  2dup dict-find ?dup if
    nip nip dict-addr @ gen-call exit
  then
  \ Unknown word
  ." Unknown word: " type cr
  1 throw ;

: start-def ( addr u -- )
  dict-add
  code-here current-word-addr !
  1 state ! ;

: end-def ( -- )
  gen-ret
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
  state @ if
    compile-token
  else
    \ Interpret mode - only allow definitions
    ." Unexpected token in interpret mode: " type cr
    1 throw
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
  slurp-file              \ -- addr u
  dup INPUT-SIZE > if
    ." File too large" cr 1 throw
  then
  dup input-len !
  input-buf swap move     \ copy to input-buf
  0 input-pos ! ;

: compile-file ( src-addr src-u out-addr out-u -- )
  2swap load-file
  0 code-pos !
  0 dict-count !
  0 cf-sp !
  0 state !
  gen-prologue
  compile-all               \ compile user's code (defines main etc)
  patch-start               \ patch forward jump to code-here (after all compiled words)
  $main dict-find           \ find user's main
  ?dup if
    dict-addr @ gen-call    \ call it
  then
  gen-cr
  gen-epilogue
  elf-header
  write-elf ;

\ Persistent string buffers (s" is transient, reuses same buffer)
create src-name 64 allot
variable src-len
create out-name 64 allot
variable out-len

: save-src ( addr u -- ) dup src-len ! src-name swap move ;
: save-out ( addr u -- ) dup out-len ! out-name swap move ;
: get-src ( -- addr u ) src-name src-len @ ;
: get-out ( -- addr u ) out-name out-len @ ;

\ Entry point - compile file from command line
\ Run from command line: fifth tf.fs
\ Input: input.fs  Output: output

: main-entry ( -- )
  s" input.fs" save-src
  s" output" save-out
  get-src get-out compile-file
  s" chmod +x output" system drop
  bye ;

main-entry
