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

