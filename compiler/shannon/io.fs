\ io.fs - I/O Code Generation (Shannon Layer 2d)
\ Ported from sixth.fs for the Shannon Architecture
\
\ Requires: asm.fs, stack.fs, c,, d,

\ ============================================================
\ EMIT - Output single character
\ ============================================================

: gen-emit ( -- )
  \ ( c -- ) Write character to stdout
  \ Save registers, push char, syscall write, restore
  stack-depth @ 2 >= if $53 c, then   \ push rbx if needed
  stack-depth @ 3 >= if $51 c, then   \ push rcx if needed
  $50 c,                              \ push rax (the char)
  $b8 c, 1 d,                         \ mov eax, 1 (SYS_write)
  $bf c, 1 d,                         \ mov edi, 1 (stdout)
  $48 c, $89 c, $e6 c,                \ mov rsi, rsp (buffer)
  $ba c, 1 d,                         \ mov edx, 1 (count)
  $0f c, $05 c,                       \ syscall
  $58 c,                              \ pop (discard char)
  stack-depth @ 3 >= if $59 c, then   \ pop rcx
  stack-depth @ 2 >= if $5b c, then   \ pop rbx
  pop-val ;

\ ============================================================
\ CR - Output newline
\ ============================================================

: gen-cr ( -- )
  \ ( -- ) Write newline to stdout
  $50 c,                              \ push rax (save TOS)
  stack-depth @ 2 >= if $53 c, then   \ push rbx if needed
  stack-depth @ 3 >= if $51 c, then   \ push rcx if needed
  $6a c, 10 c,                        \ push 10 (newline)
  $b8 c, 1 d,                         \ mov eax, 1
  $bf c, 1 d,                         \ mov edi, 1
  $48 c, $89 c, $e6 c,                \ mov rsi, rsp
  $ba c, 1 d,                         \ mov edx, 1
  $0f c, $05 c,                       \ syscall
  $58 c,                              \ pop scratch
  stack-depth @ 3 >= if $59 c, then   \ pop rcx
  stack-depth @ 2 >= if $5b c, then   \ pop rbx
  $58 c, ;                            \ pop rax (restore TOS)

\ ============================================================
\ SPACE - Output space character
\ ============================================================

: gen-space ( -- )
  \ ( -- ) Write space to stdout
  $50 c,                              \ push rax
  stack-depth @ 2 >= if $53 c, then
  stack-depth @ 3 >= if $51 c, then
  $6a c, 32 c,                        \ push 32 (space)
  $b8 c, 1 d,
  $bf c, 1 d,
  $48 c, $89 c, $e6 c,
  $ba c, 1 d,
  $0f c, $05 c,
  $58 c,
  stack-depth @ 3 >= if $59 c, then
  stack-depth @ 2 >= if $5b c, then
  $58 c, ;

\ ============================================================
\ TYPE - Output string
\ ============================================================

: gen-type ( -- )
  \ ( addr len -- ) Write string to stdout
  \ len in rax, addr in rbx
  stack-depth @ 3 >= if $51 c, then   \ save rcx if needed
  $48 c, $89 c, $c2 c,                \ mov rdx, rax (len)
  $48 c, $89 c, $de c,                \ mov rsi, rbx (addr)
  $b8 c, 1 d,                         \ mov eax, 1
  $bf c, 1 d,                         \ mov edi, 1
  $0f c, $05 c,                       \ syscall
  stack-depth @ 3 >= if $59 c, then
  pop-val pop-val ;

\ ============================================================
\ DOT - Print signed number with trailing space
\ ============================================================

: gen-dot ( -- )
  \ ( n -- ) Print signed number, space after
  stack-depth @ 3 >= if $51 c, then   \ save rcx

  \ r8d = digit count
  $45 c, $31 c, $c0 c,                \ xor r8d, r8d

  \ Check sign
  $48 c, $85 c, $c0 c,                \ test rax, rax
  $79 c, 28 c,                        \ jns skip_neg (28 bytes ahead)

  \ Negative: print '-', negate
  $50 c,                              \ push rax
  $b8 c, 1 d,                         \ mov eax, 1
  $bf c, 1 d,                         \ mov edi, 1
  $6a c, 45 c,                        \ push '-'
  $48 c, $89 c, $e6 c,                \ mov rsi, rsp
  $ba c, 1 d,                         \ mov edx, 1
  $0f c, $05 c,                       \ syscall
  $58 c,                              \ pop '-'
  $58 c,                              \ pop rax
  $48 c, $f7 c, $d8 c,                \ neg rax

  \ Division loop: push digits
  code-here
  $48 c, $c7 c, $c1 c, 10 d,          \ mov rcx, 10
  $48 c, $31 c, $d2 c,                \ xor rdx, rdx
  $48 c, $f7 c, $f1 c,                \ div rcx
  $83 c, $c2 c, $30 c,                \ add edx, '0'
  $52 c,                              \ push rdx
  $41 c, $ff c, $c0 c,                \ inc r8d
  $48 c, $85 c, $c0 c,                \ test rax, rax
  $75 c,
  dup code-here 1+ - c,               \ jnz loop
  drop

  \ Print loop: pop and write digits
  code-here
  $b8 c, 1 d,                         \ mov eax, 1
  $bf c, 1 d,                         \ mov edi, 1
  $48 c, $89 c, $e6 c,                \ mov rsi, rsp
  $ba c, 1 d,                         \ mov edx, 1
  $0f c, $05 c,                       \ syscall
  $58 c,                              \ pop digit
  $41 c, $ff c, $c8 c,                \ dec r8d
  $75 c,
  dup code-here 1+ - c,               \ jnz loop
  drop

  \ Trailing space
  $6a c, 32 c,                        \ push ' '
  $b8 c, 1 d,
  $bf c, 1 d,
  $48 c, $89 c, $e6 c,
  $ba c, 1 d,
  $0f c, $05 c,
  $58 c,                              \ pop space

  stack-depth @ 3 >= if $59 c, then   \ restore rcx
  pop-val ;

\ ============================================================
\ KEY - Read single character
\ ============================================================

: gen-key ( -- )
  \ ( -- c ) Read character from stdin
  push-val
  $48 c, $83 c, $ec c, 8 c,           \ sub rsp, 8
  $48 c, $31 c, $c0 c,                \ xor rax, rax (SYS_read)
  $48 c, $31 c, $ff c,                \ xor rdi, rdi (stdin)
  $48 c, $89 c, $e6 c,                \ mov rsi, rsp
  $ba c, 1 d,                         \ mov edx, 1
  $0f c, $05 c,                       \ syscall
  $48 c, $0f c, $b6 c, $04 c, $24 c,  \ movzx rax, byte [rsp]
  $48 c, $83 c, $c4 c, 8 c, ;         \ add rsp, 8

\ ============================================================
\ FILE OPERATIONS
\ ============================================================

\ Note: rt-word-buf, SLURP-SIZE, rt-slurp-buf defined in main.fs before this file is loaded

: gen-create-file ( -- )
  \ ( addr u fam -- fileid ior )
  \ fam in rax, u in rbx, addr in rcx (if stack-depth>=3) or [r15]
  ." DBG: gen-create-file start, depth=" stack-depth @ . cr
  \ Get addr into rsi (handle register vs memory stack)
  stack-depth @ 3 >= if
    ." DBG: using rcx" cr
    $48 c, $89 c, $ce c,              \ mov rsi, rcx (addr from 3rd reg)
  else
    ." DBG: using [r15]" cr
    $49 c, $8b c, $37 c,              \ mov rsi, [r15] (addr from memory)
    $49 c, $83 c, $c7 c, 8 c,         \ add r15, 8
  then
  ." DBG: past conditional" cr
  $50 c,                              \ push rax (save fam)
  \ Copy string: rsi=src addr, rbx=len, rdi=dest
  $48 c, $bf c, rt-word-buf q,        \ mov rdi, rt-word-buf
  $48 c, $89 c, $d9 c,                \ mov rcx, rbx (len)
  $f3 c, $a4 c,                       \ rep movsb
  $c6 c, $07 c, 0 c,                  \ mov byte [rdi], 0 (null terminate)
  $58 c,                              \ pop rax (fam -> flags)
  \ OR in O_CREAT|O_TRUNC (576) for create-file semantics
  $48 c, $0d c, 576 d,                \ or rax, 576 (O_CREAT|O_TRUNC)
  $48 c, $89 c, $c6 c,                \ mov rsi, rax (flags)
  $48 c, $bf c, rt-word-buf q,        \ mov rdi, rt-word-buf
  $ba c, 420 d,                       \ mov edx, 0644 (mode)
  $b8 c, 2 d,                         \ mov eax, 2 (sys_open)
  $0f c, $05 c,                       \ syscall
  \ rax = fd (>=0) or -errno (<0)
  $48 c, $89 c, $c3 c,                \ mov rbx, rax (save fd to NOS)
  $48 c, $31 c, $c0 c,                \ xor rax, rax (ior = 0)
  $48 c, $85 c, $db c,                \ test rbx, rbx
  $79 c, 6 c,                         \ jns +6 (skip error handling)
  $48 c, $31 c, $db c,                \ xor rbx, rbx (fd = 0 on error)
  $48 c, $ff c, $c8 c,                \ dec rax (ior = -1)
  \ Stack: was (addr u fam), now (fd ior) - consumed 3, produced 2
  pop-val ;                           \ adjust stack depth by 1

: gen-close-file ( -- )
  \ ( fileid -- ior )
  $48 c, $89 c, $c7 c,                \ mov rdi, rax (fd)
  $b8 c, 3 d,                         \ mov eax, 3 (sys_close)
  $0f c, $05 c,                       \ syscall
  \ rax = 0 on success, -errno on failure
  $48 c, $85 c, $c0 c,                \ test rax, rax
  $74 c, 7 c,                         \ jz +7 (skip if success)
  $48 c, $c7 c, $c0 c, -1 d, ;        \ mov rax, -1

: gen-write-file ( -- )
  \ ( addr u fileid -- ior )
  \ fileid=rax, u=rbx, addr=rcx
  $48 c, $89 c, $c7 c,                \ mov rdi, rax (fd)
  $48 c, $89 c, $da c,                \ mov rdx, rbx (count)
  $48 c, $89 c, $ce c,                \ mov rsi, rcx (addr)
  $b8 c, 1 d,                         \ mov eax, 1 (sys_write)
  $0f c, $05 c,                       \ syscall
  \ rax = bytes written or -errno
  $48 c, $85 c, $c0 c,                \ test rax, rax
  $79 c, 7 c,                         \ jns +7 (skip if success)
  $48 c, $c7 c, $c0 c, -1 d,          \ mov rax, -1
  pop-val pop-val ;                   \ consume addr and u, leave ior

\ ============================================================
\ SLURP-FILE - Read entire file into buffer
\ ============================================================

: emit-slurp-file ( -- )
  \ ( addr u -- addr2 u2 ior )
  \ Reads file into rt-slurp-buf. Returns addr2=buffer, u2=bytes, ior=0 on success.
  \ On error, returns 0 0 -1.
  \ Input: TOS=rax=u (len), NOS=rbx=addr (path)
  \ Output: TOS=rax=ior, NOS=rbx=u2, 3rd=rcx=addr2

  \ Copy path to rt-word-buf and null-terminate
  $48 c, $bf c, rt-word-buf q,             \ mov rdi, rt-word-buf
  $48 c, $89 c, $de c,                     \ mov rsi, rbx (addr)
  $48 c, $89 c, $c1 c,                     \ mov rcx, rax (len)
  $f3 c, $a4 c,                            \ rep movsb
  $c6 c, $07 c, 0 c,                       \ mov byte [rdi], 0

  \ Open syscall (SYS_open=2)
  $48 c, $bf c, rt-word-buf q,             \ mov rdi, rt-word-buf
  $31 c, $f6 c,                            \ xor esi, esi (O_RDONLY)
  $31 c, $d2 c,                            \ xor edx, edx
  $b8 c, 2 d,                              \ mov eax, 2
  $0f c, $05 c,                            \ syscall
  $48 c, $85 c, $c0 c,                     \ test rax, rax
  $78 c, 51 c,                             \ js +51 to error (open failed)

  \ Save fd on machine stack
  $57 c,                                   \ push rdi (save for later)
  $48 c, $89 c, $c7 c,                     \ mov rdi, rax (fd)
  $57 c,                                   \ push rdi (fd for close)

  \ Read syscall (SYS_read=0)
  $48 c, $be c, rt-slurp-buf q,            \ mov rsi, rt-slurp-buf
  $ba c, SLURP-SIZE d,                     \ mov edx, SLURP-SIZE
  $31 c, $c0 c,                            \ xor eax, eax
  $0f c, $05 c,                            \ syscall
  $50 c,                                   \ push rax (bytes read)

  \ Close syscall (SYS_close=3)
  $48 c, $8b c, $7c c, $24 c, 8 c,         \ mov rdi, [rsp+8] (fd)
  $b8 c, 3 d,                              \ mov eax, 3
  $0f c, $05 c,                            \ syscall

  \ Return success: (rt-slurp-buf, bytes, 0)
  \ Need: rax=ior(0), rbx=u2(bytes), rcx=addr2(rt-slurp-buf)
  $58 c,                                   \ pop rax (bytes read)
  $48 c, $83 c, $c4 c, 16 c,               \ add rsp, 16 (discard fd, saved rdi)
  $48 c, $89 c, $c3 c,                     \ mov rbx, rax (u2 = bytes)
  $48 c, $b9 c, rt-slurp-buf q,            \ mov rcx, rt-slurp-buf (addr2)
  $31 c, $c0 c,                            \ xor eax, eax (ior=0)
  $eb c, 12 c,                             \ jmp +12 to end

  \ Error: return (0, 0, -1)
  \ Need: rax=-1, rbx=0, rcx=0
  $31 c, $db c,                            \ xor ebx, ebx (u2=0)
  $31 c, $c9 c,                            \ xor ecx, ecx (addr2=0)
  $48 c, $c7 c, $c0 c, -1 d,               \ mov rax, -1 (ior=-1)

  \ Adjust stack depth: 2 inputs -> 3 outputs = +1
  1 stack-depth+! ;

