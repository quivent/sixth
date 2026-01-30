\ ff.fs - Minimal Fifth-to-x86_64 compiler
\ TOS in rax, NOS in rbx (callee-saved), rest at [r15]

\ ============================================================
\ 1. BUFFERS
\ ============================================================

4096 constant CODE-SIZE
create code-buf CODE-SIZE allot
variable code-pos  0 code-pos !

4096 constant INPUT-SIZE
create input-buf INPUT-SIZE allot
variable input-len  0 input-len !
variable input-pos  0 input-pos !

create token-buf 64 allot
variable token-len  0 token-len !

\ Dictionary: 16 entries, 32 bytes each (24 name + 8 addr)
create dict-buf 512 allot
variable dict-count  0 dict-count !

variable compiling  0 compiling !

\ ============================================================
\ 2. CODE EMISSION
\ ============================================================

: c, ( b -- ) code-buf code-pos @ + c!  1 code-pos +! ;
: d, ( dw -- ) dup c, 8 rshift dup c, 8 rshift dup c, 8 rshift c, ;
: here ( -- n ) code-pos @ ;

\ ============================================================
\ 3. CODE GENERATORS (TOS=rax, NOS=rbx callee-saved, stack=[r15])
\ ============================================================

: gen-lit ( n -- )
  $49 c, $83 c, $ef c, 8 c,     \ sub r15, 8
  $49 c, $89 c, $1f c,          \ mov [r15], rbx
  $48 c, $89 c, $c3 c,          \ mov rbx, rax
  dup $7fffffff > if
    $48 c, $b8 c,               \ mov rax, imm64
    dup d, 32 rshift d,
  else
    $b8 c, d,                   \ mov eax, imm32 (5 bytes, zero-extends)
  then ;

: gen-add ( -- )
  $48 c, $01 c, $d8 c,          \ add rax, rbx
  $49 c, $8b c, $1f c,          \ mov rbx, [r15]
  $49 c, $83 c, $c7 c, 8 c, ;   \ add r15, 8

: gen-sub ( -- )
  $48 c, $93 c,                 \ xchg rax, rbx
  $48 c, $29 c, $d8 c,          \ sub rax, rbx
  $49 c, $8b c, $1f c,          \ mov rbx, [r15]
  $49 c, $83 c, $c7 c, 8 c, ;   \ add r15, 8

: gen-mul ( -- )
  $48 c, $0f c, $af c, $c3 c,   \ imul rax, rbx
  $49 c, $8b c, $1f c,          \ mov rbx, [r15]
  $49 c, $83 c, $c7 c, 8 c, ;   \ add r15, 8

: gen-div ( -- )
  $48 c, $93 c,                 \ xchg rax, rbx
  $31 c, $d2 c,                 \ xor edx, edx
  $48 c, $f7 c, $f3 c,          \ div rbx
  $49 c, $8b c, $1f c,          \ mov rbx, [r15]
  $49 c, $83 c, $c7 c, 8 c, ;   \ add r15, 8

: gen-mod ( -- )
  $48 c, $93 c,                 \ xchg rax, rbx
  $31 c, $d2 c,                 \ xor edx, edx
  $48 c, $f7 c, $f3 c,          \ div rbx
  $89 c, $d0 c,                 \ mov eax, edx
  $49 c, $8b c, $1f c,          \ mov rbx, [r15]
  $49 c, $83 c, $c7 c, 8 c, ;   \ add r15, 8

: gen-negate ( -- )
  $48 c, $f7 c, $d8 c, ;        \ neg rax

: gen-1+ ( -- )
  $48 c, $ff c, $c0 c, ;        \ inc rax

: gen-1- ( -- )
  $48 c, $ff c, $c8 c, ;        \ dec rax

: gen-dup ( -- )
  $49 c, $83 c, $ef c, 8 c,   \ sub r15, 8
  $49 c, $89 c, $1f c,        \ mov [r15], rbx
  $48 c, $89 c, $c3 c, ;      \ mov rbx, rax

: gen-over ( -- )
  $49 c, $83 c, $ef c, 8 c,     \ sub r15, 8
  $49 c, $89 c, $1f c,          \ mov [r15], rbx
  $48 c, $93 c, ;               \ xchg rax, rbx

: gen-swap ( -- )
  $48 c, $93 c, ;               \ xchg rax, rbx

: gen-drop ( -- )
  $48 c, $89 c, $d8 c,          \ mov rax, rbx
  $49 c, $8b c, $1f c,          \ mov rbx, [r15]
  $49 c, $83 c, $c7 c, 8 c, ;   \ add r15, 8

: gen-rot ( -- )  \ ( a b c -- b c a )
  $49 c, $87 c, $1f c,          \ xchg rbx, [r15]
  $48 c, $93 c, ;               \ xchg rax, rbx

: gen-nip ( -- )  \ ( a b -- b )
  $49 c, $8b c, $1f c,          \ mov rbx, [r15]
  $49 c, $83 c, $c7 c, 8 c, ;   \ add r15, 8

: gen-tuck ( -- )  \ ( a b -- b a b )
  $49 c, $83 c, $ef c, 8 c,     \ sub r15, 8
  $49 c, $89 c, $07 c, ;        \ mov [r15], rax

: gen-2dup ( -- )  \ ( a b -- a b a b )
  $49 c, $83 c, $ef c, 8 c,     \ sub r15, 8
  $49 c, $89 c, $1f c,          \ mov [r15], rbx
  $49 c, $83 c, $ef c, 8 c,     \ sub r15, 8
  $49 c, $89 c, $07 c, ;        \ mov [r15], rax

: gen-2drop ( -- )  \ ( a b -- )
  $49 c, $8b c, $07 c,          \ mov rax, [r15]
  $49 c, $8b c, $5f c, 8 c,     \ mov rbx, [r15+8]
  $49 c, $83 c, $c7 c, $10 c, ; \ add r15, 16

: gen-eq ( -- )  \ ( a b -- flag )
  $48 c, $39 c, $d8 c,          \ cmp rax, rbx
  $0f c, $94 c, $c0 c,          \ sete al
  $48 c, $0f c, $b6 c, $c0 c,   \ movzx rax, al
  $48 c, $f7 c, $d8 c,          \ neg rax
  $49 c, $8b c, $1f c,          \ mov rbx, [r15]
  $49 c, $83 c, $c7 c, 8 c, ;   \ add r15, 8

: gen-ne ( -- )  \ <>
  $48 c, $39 c, $d8 c,          \ cmp rax, rbx
  $0f c, $95 c, $c0 c,          \ setne al
  $48 c, $0f c, $b6 c, $c0 c,
  $48 c, $f7 c, $d8 c,
  $49 c, $8b c, $1f c,
  $49 c, $83 c, $c7 c, 8 c, ;

: gen-lt ( -- )  \ <
  $48 c, $39 c, $c3 c,          \ cmp rbx, rax
  $0f c, $9c c, $c0 c,          \ setl al
  $48 c, $0f c, $b6 c, $c0 c,
  $48 c, $f7 c, $d8 c,
  $49 c, $8b c, $1f c,
  $49 c, $83 c, $c7 c, 8 c, ;

: gen-gt ( -- )  \ >
  $48 c, $39 c, $c3 c,          \ cmp rbx, rax
  $0f c, $9f c, $c0 c,          \ setg al
  $48 c, $0f c, $b6 c, $c0 c,
  $48 c, $f7 c, $d8 c,
  $49 c, $8b c, $1f c,
  $49 c, $83 c, $c7 c, 8 c, ;

: gen-le ( -- )
  $48 c, $39 c, $c3 c,
  $0f c, $9e c, $c0 c,
  $48 c, $0f c, $b6 c, $c0 c,
  $48 c, $f7 c, $d8 c,
  $49 c, $8b c, $1f c,
  $49 c, $83 c, $c7 c, 8 c, ;

: gen-ge ( -- )
  $48 c, $39 c, $c3 c,
  $0f c, $9d c, $c0 c,
  $48 c, $0f c, $b6 c, $c0 c,
  $48 c, $f7 c, $d8 c,
  $49 c, $8b c, $1f c,
  $49 c, $83 c, $c7 c, 8 c, ;

: gen-0= ( -- )
  $48 c, $85 c, $c0 c,          \ test rax, rax
  $0f c, $94 c, $c0 c,          \ sete al
  $48 c, $0f c, $b6 c, $c0 c,
  $48 c, $f7 c, $d8 c, ;

: gen-0< ( -- )
  $48 c, $c1 c, $f8 c, 63 c, ;  \ sar rax, 63

: gen-and ( -- )
  $48 c, $21 c, $d8 c,          \ and rax, rbx
  $49 c, $8b c, $1f c,
  $49 c, $83 c, $c7 c, 8 c, ;

: gen-or ( -- )
  $48 c, $09 c, $d8 c,          \ or rax, rbx
  $49 c, $8b c, $1f c,
  $49 c, $83 c, $c7 c, 8 c, ;

: gen-xor ( -- )
  $48 c, $31 c, $d8 c,          \ xor rax, rbx
  $49 c, $8b c, $1f c,
  $49 c, $83 c, $c7 c, 8 c, ;

: gen-invert ( -- )
  $48 c, $f7 c, $d0 c, ;        \ not rax

\ Peephole patterns
: gen-dup-1- ( -- )
  $49 c, $83 c, $ef c, 8 c,   \ sub r15, 8
  $49 c, $89 c, $1f c,        \ mov [r15], rbx
  $48 c, $89 c, $c3 c,        \ mov rbx, rax
  $48 c, $ff c, $c8 c, ;      \ dec rax

: gen-dup-1+ ( -- )
  $49 c, $83 c, $ef c, 8 c,
  $49 c, $89 c, $1f c,
  $48 c, $89 c, $c3 c,
  $48 c, $ff c, $c0 c, ;

\ swap 2 - : xchg + sub (pattern optimization)
: gen-swap-2- ( -- )
  $48 c, $93 c,                 \ xchg rax, rbx
  $48 c, $83 c, $e8 c, 2 c, ;   \ sub rax, 2

\ dup N < : combined (saves ~20 bytes)
: gen-dup-lit-lt ( n -- )
  $49 c, $83 c, $ef c, 8 c,     \ sub r15, 8
  $49 c, $89 c, $1f c,          \ mov [r15], rbx
  $48 c, $89 c, $c3 c,          \ mov rbx, rax
  dup 128 < if
    $48 c, $83 c, $f8 c, c,     \ cmp rax, imm8
  else
    $48 c, $3d c, d,            \ cmp rax, imm32
  then
  $0f c, $9c c, $c0 c,          \ setl al
  $48 c, $0f c, $b6 c, $c0 c,   \ movzx rax, al
  $48 c, $f7 c, $d8 c, ;        \ neg rax

\ Control flow stack
create cf-stack 64 cells allot
variable cf-sp  0 cf-sp !
: cf-push ( n -- ) cf-stack cf-sp @ cells + !  1 cf-sp +! ;
: cf-pop ( -- n ) -1 cf-sp +!  cf-stack cf-sp @ cells + @ ;

: patch-fwd ( addr -- )
  here over - 1 -
  dup $7f > if ." jump too far" cr then
  swap code-buf + c! ;

: gen-if ( -- )
  $48 c, $89 c, $c2 c,          \ mov rdx, rax (save condition)
  $48 c, $89 c, $d8 c,          \ mov rax, rbx
  $49 c, $8b c, $1f c,          \ mov rbx, [r15]
  $4d c, $8d c, $7f c, 8 c,     \ lea r15, [r15+8] (no flag clobber)
  $48 c, $85 c, $d2 c,          \ test rdx, rdx
  $74 c,                        \ jz rel8
  here cf-push
  0 c, ;

: gen-else ( -- )
  $eb c,
  here
  0 c,
  cf-pop patch-fwd
  cf-push ;

: gen-then ( -- )
  cf-pop patch-fwd ;

: gen-dot ( -- )
  $53 c,                        \ push rbx (save NOS)
  $48 c, $85 c, $c0 c,          \ test rax, rax
  $79 c, $1c c,                 \ jns +28
  $50 c,                        \ push rax
  $6a c, $2d c,                 \ push '-'
  $b8 c, 1 d, $bf c, 1 d,
  $48 c, $89 c, $e6 c,
  $ba c, 1 d,
  $0f c, $05 c,
  $58 c, $58 c,
  $48 c, $f7 c, $d8 c,          \ neg rax
  $b9 c, 10 d,                  \ mov ecx, 10
  $45 c, $31 c, $c0 c,
  $31 c, $d2 c,
  $f7 c, $f1 c,
  $83 c, $c2 c, $30 c,
  $52 c,
  $41 c, $ff c, $c0 c,
  $85 c, $c0 c,
  $75 c, $f1 c,
  $b8 c, 1 d, $bf c, 1 d,
  $48 c, $89 c, $e6 c,
  $ba c, 1 d,
  $0f c, $05 c,
  $58 c,
  $41 c, $ff c, $c8 c,
  $75 c, $e6 c,
  $6a c, 32 c,
  $b8 c, 1 d, $bf c, 1 d,
  $48 c, $89 c, $e6 c, $ba c, 1 d,
  $0f c, $05 c, $58 c,
  \ pop stack: rax=rbx, rbx=[r15]
  $5b c,                        \ pop rbx (restore saved NOS, now is new TOS conceptually)
  $48 c, $89 c, $d8 c,          \ mov rax, rbx
  $49 c, $8b c, $1f c,          \ mov rbx, [r15]
  $49 c, $83 c, $c7 c, 8 c, ;   \ add r15, 8

: gen-cr ( -- )
  $53 c,                        \ push rbx (save NOS)
  $50 c,                        \ push rax (save TOS)
  $6a c, 10 c,
  $b8 c, 1 d, $bf c, 1 d,
  $48 c, $89 c, $e6 c, $ba c, 1 d,
  $0f c, $05 c,
  $58 c, $58 c, $5b c, ;        \ pop newline, pop rax, pop rbx

: gen-call ( addr -- )
  $e8 c,                        \ call rel32
  here 4 + - d, ;               \ rbx is callee-saved, no push/pop needed

\ Fast entry: skip push/pop for base case (n < 2)
variable fast-ret  0 fast-ret !

: gen-func-entry ( -- )
  0 fast-ret !                  \ reset for this function
  $48 c, $83 c, $f8 c, 2 c,     \ cmp rax, 2
  $7c c,                        \ jl quick_ret
  here fast-ret !               \ save patch location
  0 c,                          \ placeholder
  $53 c, ;                      \ push rbx

: gen-ret ( -- )
  $5b c, $c3 c,                 \ pop rbx; ret (normal path)
  fast-ret @ ?dup if
    here over - 1-              \ offset = here - fast-ret - 1
    dup 128 < if                \ only if fits in rel8
      swap code-buf + c!        \ patch the jl
    else
      drop                      \ offset too large, fall through
      0 swap code-buf + c!      \ jl +0 -> just continue to push
    then
  then
  $c3 c, ;                      \ ret (quick path)

: gen-prologue ( -- )
  $49 c, $bf c,                 \ mov r15, imm64
  0 c, $10 c, $40 c, 0 c,
  0 c, 0 c, 0 c, 0 c, ;

: gen-epilogue ( -- )
  $b8 c, 60 d,
  $31 c, $ff c,
  $0f c, $05 c, ;

variable jmp-patch  0 jmp-patch !

: gen-jmp-fwd ( -- )
  $e9 c,
  here jmp-patch !
  0 d, ;

: patch-jmp ( -- )
  here jmp-patch @ - 4 -
  dup jmp-patch @ code-buf + c!
  8 rshift dup jmp-patch @ 1+ code-buf + c!
  8 rshift dup jmp-patch @ 2 + code-buf + c!
  8 rshift jmp-patch @ 3 + code-buf + c! ;

\ ============================================================
\ 4. TOKENIZER
\ ============================================================

: skip-ws ( -- )
  begin
    input-pos @ input-len @ < while
    input-buf input-pos @ + c@ 33 < if
      1 input-pos +!
    else exit then
  repeat ;

: skip-line ( -- )
  begin
    input-pos @ input-len @ < while
    input-buf input-pos @ + c@ 10 = if
      1 input-pos +! exit
    then
    1 input-pos +!
  repeat ;

: get-token ( -- addr u )
  begin
    skip-ws
    input-pos @ input-len @ < 0= if 0 0 exit then
    input-buf input-pos @ + c@ [char] \ = if
      skip-line
    else
      0 token-len !
      begin
        input-pos @ input-len @ < while
        input-buf input-pos @ + c@
        dup 33 < if drop token-buf token-len @ exit then
        token-buf token-len @ + c!
        1 token-len +!
        1 input-pos +!
        token-len @ 64 < 0= if token-buf token-len @ exit then
      repeat
      token-buf token-len @ exit
    then
  again ;

create token2-buf 64 allot
variable token2-len  0 token2-len !
variable saved-pos   0 saved-pos !

: peek-token ( -- addr u )
  input-pos @ saved-pos !
  begin
    skip-ws
    input-pos @ input-len @ < 0= if
      saved-pos @ input-pos !
      0 0 exit
    then
    input-buf input-pos @ + c@ [char] \ = if
      skip-line
    else
      0 token2-len !
      begin
        input-pos @ input-len @ < while
        input-buf input-pos @ + c@
        dup 33 < if
          drop
          saved-pos @ input-pos !
          token2-buf token2-len @ exit
        then
        token2-buf token2-len @ + c!
        1 token2-len +!
        1 input-pos +!
        token2-len @ 64 < 0= if
          saved-pos @ input-pos !
          token2-buf token2-len @ exit
        then
      repeat
      saved-pos @ input-pos !
      token2-buf token2-len @ exit
    then
  again ;

\ ============================================================
\ 5. TOKEN MATCHING
\ ============================================================
require compiler/lib.fs

\ ============================================================
\ 6. NUMBER PARSING
\ ============================================================

: digit? ( c -- n t | f )
  dup [char] 0 < if drop false exit then
  dup [char] 9 > if drop false exit then
  [char] 0 - true ;

: parse-num ( a u -- n t | f )
  dup 0= if 2drop false exit then
  0 >r
  over c@ [char] - = if
    1 /string dup 0= if 2drop r> drop false exit then
    begin dup 0> while
      over c@ digit? if r> 10 * + >r 1 /string
      else 2drop r> drop false exit then
    repeat 2drop r> negate true exit
  then
  begin dup 0> while
    over c@ digit? if r> 10 * + >r 1 /string
    else 2drop r> drop false exit then
  repeat 2drop r> true ;

\ ============================================================
\ 7. DICTIONARY
\ ============================================================

: dict-entry ( -- addr ) dict-buf dict-count @ 32 * + ;

: dict-add ( a u -- )
  dict-entry swap move
  dict-entry 24 + here swap !
  1 dict-count +! ;

: name= ( a1 u1 a2 -- ? )
  >r 2dup r> swap
  dup 0= if drop 2drop true exit then
  0 do
    over i + c@ over i + c@ <> if 2drop drop false unloop exit then
  loop
  2drop drop true ;

: dict-find ( a u -- addr | 0 )
  dict-count @ dup 0= if drop 2drop 0 exit then
  0 do
    2dup dict-buf i 32 * + name= if
      2drop dict-buf i 32 * + 24 + @ unloop exit
    then
  loop
  2drop 0 ;

\ ============================================================
\ 8. COMPILER
\ ============================================================

: compile-token ( a u -- )
  2dup is-+? if 2drop gen-add exit then
  2dup is--? if 2drop gen-sub exit then
  2dup is-*? if 2drop gen-mul exit then
  2dup is-/? if 2drop gen-div exit then
  2dup is-mod? if 2drop gen-mod exit then
  2dup is-negate? if 2drop gen-negate exit then
  2dup is-1+? if 2drop gen-1+ exit then
  2dup is-1-? if 2drop gen-1- exit then
  2dup is-dup? if
    peek-token dup 0> if
      2dup is-1-? if 2drop get-token 2drop 2drop gen-dup-1- exit then
      2dup is-1+? if 2drop get-token 2drop 2drop gen-dup-1+ exit then
      2drop
    else 2drop then
    2drop gen-dup exit
  then
  2dup is-over? if 2drop gen-over exit then
  \ Pattern: swap 2 -
  2dup is-swap? if
    peek-token dup 0> if
      2dup is-2? if
        2drop get-token 2drop   \ consume "2"
        peek-token dup 0> if
          2dup is--? if
            2drop get-token 2drop  \ consume "-"
            2drop gen-swap-2- exit
          then
          2drop
        else 2drop then
        \ didn't match full pattern, emit swap + literal 2
        2drop gen-swap 2 gen-lit exit
      then
      2drop
    else 2drop then
    2drop gen-swap exit
  then
  2dup is-drop? if 2drop gen-drop exit then
  2dup is-rot? if 2drop gen-rot exit then
  2dup is-nip? if 2drop gen-nip exit then
  2dup is-tuck? if 2drop gen-tuck exit then
  2dup is-2dup? if 2drop gen-2dup exit then
  2dup is-2drop? if 2drop gen-2drop exit then
  2dup is-=? if 2drop gen-eq exit then
  2dup is-<>? if 2drop gen-ne exit then
  2dup is-<=? if 2drop gen-le exit then
  2dup is->=? if 2drop gen-ge exit then
  2dup is-<? if 2drop gen-lt exit then
  2dup is->? if 2drop gen-gt exit then
  2dup is-0=? if 2drop gen-0= exit then
  2dup is-0<? if 2drop gen-0< exit then
  2dup is-and? if 2drop gen-and exit then
  2dup is-or? if 2drop gen-or exit then
  2dup is-xor? if 2drop gen-xor exit then
  2dup is-invert? if 2drop gen-invert exit then
  2dup is-if? if 2drop gen-if exit then
  2dup is-else? if 2drop gen-else exit then
  2dup is-then? if 2drop gen-then exit then
  2dup is-.? if 2drop gen-dot exit then
  2dup is-cr? if 2drop gen-cr exit then
  2dup parse-num if nip nip gen-lit exit then
  2dup dict-find ?dup if nip nip gen-call exit then
  ." ?" type cr ;

: compile-word ( a u -- )
  2dup is-:? if
    2drop get-token dict-add
    gen-func-entry
    1 compiling ! exit
  then
  2dup is-;? if
    2drop gen-ret
    0 compiling ! exit
  then
  compiling @ if compile-token else 2drop then ;

: compile-all ( -- )
  begin
    get-token dup 0> while
    compile-word
  repeat 2drop ;

\ ============================================================
\ 9. ELF OUTPUT
\ ============================================================

create elf-hdr 120 allot

: make-elf ( -- )
  elf-hdr 120 0 fill
  $7f elf-hdr c!
  [char] E elf-hdr 1+ c!
  [char] L elf-hdr 2 + c!
  [char] F elf-hdr 3 + c!
  2 elf-hdr 4 + c!
  1 elf-hdr 5 + c!
  1 elf-hdr 6 + c!
  2 elf-hdr $10 + c!
  $3e elf-hdr $12 + c!
  1 elf-hdr $14 + c!
  $400078 elf-hdr $18 + !
  64 elf-hdr $20 + !
  64 elf-hdr $34 + c!
  56 elf-hdr $36 + c!
  1 elf-hdr $38 + c!
  1 elf-hdr 64 + !
  7 elf-hdr 68 + !
  0 elf-hdr 72 + !
  $400000 elf-hdr 80 + !
  $400000 elf-hdr 88 + !
  120 here + elf-hdr 96 + !
  $2000 elf-hdr 104 + !
  $1000 elf-hdr 112 + ! ;

: write-elf ( a u -- )
  w/o create-file throw >r
  elf-hdr 120 r@ write-file throw
  code-buf here r@ write-file throw
  r> close-file throw ;

\ ============================================================
\ 10. MAIN
\ ============================================================

variable main-addr  0 main-addr !

: find-main ( -- )
  dict-count @ dup 0= if drop exit then
  0 do
    dict-buf i 32 * + c@ [char] m = if
      dict-buf i 32 * + 1+ c@ [char] a = if
        dict-buf i 32 * + 2 + c@ [char] i = if
          dict-buf i 32 * + 3 + c@ [char] n = if
            dict-buf i 32 * + 24 + @ main-addr !
            unloop exit
          then
        then
      then
    then
  loop ;

create src-buf 32 allot
create out-buf 32 allot

: compile-file ( -- )
  s" test.fs" src-buf swap move
  src-buf 7 slurp-file
  dup input-len ! input-buf swap move
  0 input-pos !
  0 code-pos !
  0 dict-count !
  0 compiling !
  gen-prologue
  gen-jmp-fwd
  compile-all
  patch-jmp
  find-main
  main-addr @ ?dup if gen-call then
  gen-epilogue
  make-elf
  s" a.out" out-buf swap move
  out-buf 5 write-elf
  s" chmod +x a.out" system drop ;

compile-file
bye
