\ mandelbrot.fs - ASCII Mandelbrot set using fixed-point arithmetic
\ Uses 12-bit fixed-point (4096 = 1.0)

create code-buf 4096 allot
variable code-pos  0 code-pos !
create elf-buf 256 allot
variable elf-pos  0 elf-pos !

: c, code-buf code-pos @ + c!  1 code-pos +! ;
: d, dup c, 8 rshift dup c, 8 rshift dup c, 8 rshift c, ;
: e, elf-buf elf-pos @ + c!  1 elf-pos +! ;
: e2, dup e, 8 rshift e, ;
: e4, dup e2, 16 rshift e2, ;
: e8, dup e4, 32 rshift e4, ;

: elf-header
  0 elf-pos !
  $7f e, 69 e, 76 e, 70 e,
  2 e, 1 e, 1 e, 0 e,
  0 e8,
  2 e2, $3e e2, 1 e4,
  $400078 e8,
  64 e8, 0 e8, 0 e4,
  64 e2, 56 e2, 1 e2,
  0 e2, 0 e2, 0 e2,
  1 e4, 5 e4, 0 e8,
  $400000 e8, $400000 e8,
  120 code-pos @ + dup e8, e8,
  $1000 e8, ;

\ Byte-by-byte trace:
\  0: mov r12d, -4096  (6)
\  6: row_loop: mov r13d, -8192  (6)
\ 12: col_loop: xor r14d, r14d  (3)
\ 15: xor r15d, r15d  (3)
\ 18: xor ebx, ebx  (2)
\ 20: iter_loop: mov r8d, r14d  (3)
\ 23: mov r9d, r15d  (3)
\ 26: mov eax, r14d  (3)
\ 29: imul eax, eax  (3)
\ 32: mov ecx, r15d  (3)
\ 35: imul ecx, ecx  (3)
\ 38: mov edx, eax  (2)
\ 40: add edx, ecx  (2)
\ 42: sar edx, 12  (3)
\ 45: cmp edx, 16384  (6)
\ 51: jg escaped (+42 to 95)  (2)
\ 53: sub eax, ecx  (2)
\ 55: sar eax, 12  (3)
\ 58: add eax, r13d  (3)
\ 61: mov r14d, eax  (3)
\ 64: mov eax, r8d  (3)
\ 67: imul eax, r9d  (4)
\ 71: add eax, eax  (2)
\ 73: sar eax, 12  (3)
\ 76: add eax, r12d  (3)
\ 79: mov r15d, eax  (3)
\ 82: inc ebx  (2)
\ 84: cmp ebx, 50  (3)
\ 87: jl iter_loop  (-69 to 20)  (2)
\ 89: mov al, 32  (2)
\ 91: jmp print_char (+22 to 115)  (2)
\ 93: escaped: mov eax, ebx  (2)
\ 95: and eax, 7  (3)
\ 98: lea rcx, [rip+6]  (7) -> points to 111 (char table)
\ 105: mov al, [rcx+rax]  (3)
\ 108: jmp print_char (+5 to 115)  (2)
\ Wait, that's wrong. Let me recalculate.

: gen-code
  0 code-pos !

  \ 0: cy = -4096 (-1.0)
  $41 c, $bc c, $00 c, $f0 c, $ff c, $ff c,  \ 6 bytes

  \ 6: row_loop - cx = -8192 (-2.0)
  $41 c, $bd c, $00 c, $e0 c, $ff c, $ff c,  \ 6 bytes

  \ 12: col_loop - zr = zi = 0, iter = 0
  $45 c, $31 c, $f6 c,              \ xor r14d, r14d (3)
  $45 c, $31 c, $ff c,              \ xor r15d, r15d (3)
  $31 c, $db c,                     \ xor ebx, ebx (2)

  \ 20: iter_loop - save zr, zi
  $45 c, $89 c, $f0 c,              \ mov r8d, r14d (3)
  $45 c, $89 c, $f9 c,              \ mov r9d, r15d (3)

  \ zr^2 in eax
  $44 c, $89 c, $f0 c,              \ mov eax, r14d (3)
  $0f c, $af c, $c0 c,              \ imul eax, eax (3)

  \ zi^2 in ecx
  $44 c, $89 c, $f9 c,              \ mov ecx, r15d (3)
  $0f c, $af c, $c9 c,              \ imul ecx, ecx (3)

  \ |z|^2 = (zr^2 + zi^2) >> 12
  $89 c, $c2 c,                     \ mov edx, eax (2)
  $01 c, $ca c,                     \ add edx, ecx (2)
  $c1 c, $fa c, 12 c,               \ sar edx, 12 (3)
  $81 c, $fa c, $00 c, $40 c, $00 c, $00 c,  \ cmp edx, 16384 (6)
  $7f c, 40 c,                      \ jg escaped (2) -> offset 40 to 93

  \ new_zr = (zr^2 - zi^2) >> 12 + cx
  $29 c, $c8 c,                     \ sub eax, ecx (2)
  $c1 c, $f8 c, 12 c,               \ sar eax, 12 (3)
  $44 c, $01 c, $e8 c,              \ add eax, r13d (3)
  $41 c, $89 c, $c6 c,              \ mov r14d, eax (3)

  \ new_zi = 2*zr*zi >> 12 + cy
  $44 c, $89 c, $c0 c,              \ mov eax, r8d (3)
  $41 c, $0f c, $af c, $c1 c,       \ imul eax, r9d (4)
  $01 c, $c0 c,                     \ add eax, eax (2)
  $c1 c, $f8 c, 12 c,               \ sar eax, 12 (3)
  $44 c, $01 c, $e0 c,              \ add eax, r12d (3)
  $41 c, $89 c, $c7 c,              \ mov r15d, eax (3)

  \ iter++, check < 50
  $ff c, $c3 c,                     \ inc ebx (2)
  $83 c, $fb c, 50 c,               \ cmp ebx, 50 (3)
  $7c c, $bb c,                     \ jl iter_loop (2) = -69

  \ In set - print space
  $b0 c, 32 c,                      \ mov al, 32 (2)
  $eb c, 25 c,                      \ jmp print_char (2) -> offset 25 to 118

  \ 95: escaped - print char based on iter % 8
  $89 c, $d8 c,                     \ mov eax, ebx (2)
  $83 c, $e0 c, 7 c,                \ and eax, 7 (3)
  $48 c, $8d c, $0d c, 5 c, 0 c, 0 c, 0 c,  \ lea rcx, [rip+5] -> 110 (7)
  $8a c, $04 c, $01 c,              \ mov al, [rcx+rax] (3)
  $eb c, 8 c,                       \ jmp print_char (2) -> offset 8 to 118

  \ 113: Char table (8 bytes)
  46 c, 58 c, 45 c, 61 c, 43 c, 42 c, 35 c, 64 c,

  \ 121: print_char
  $50 c,                            \ push rax (1)
  $b8 c, 1 d,                       \ mov eax, 1 (5)
  $bf c, 1 d,                       \ mov edi, 1 (5)
  $48 c, $89 c, $e6 c,              \ mov rsi, rsp (3)
  $ba c, 1 d,                       \ mov edx, 1 (5)
  $0f c, $05 c,                     \ syscall (2)
  $58 c,                            \ pop rax (1)

  \ cx += 150
  $41 c, $81 c, $c5 c, 150 d,       \ add r13d, 150 (7)
  $41 c, $81 c, $fd c, $00 c, $10 c, $00 c, $00 c,  \ cmp r13d, 4096 (7)
  $0f c, $8c c, $6c c, $ff c, $ff c, $ff c,  \ jl col_loop (6) -> -148 to 12

  \ Print newline
  $6a c, 10 c,                      \ push 10 (2)
  $b8 c, 1 d,                       \ mov eax, 1 (5)
  $bf c, 1 d,                       \ mov edi, 1 (5)
  $48 c, $89 c, $e6 c,              \ mov rsi, rsp (3)
  $ba c, 1 d,                       \ mov edx, 1 (5)
  $0f c, $05 c,                     \ syscall (2)
  $58 c,                            \ pop rax (1)

  \ cy += 341
  $41 c, $81 c, $c4 c, 341 d,       \ add r12d, 341 (7)
  $41 c, $81 c, $fc c, $00 c, $10 c, $00 c, $00 c,  \ cmp r12d, 4096 (7)
  $0f c, $8c c, $3b c, $ff c, $ff c, $ff c,  \ jl row_loop (6) -> -197 to 6

  \ Exit
  $b8 c, 60 d,                      \ mov eax, 60 (5)
  $31 c, $ff c,                     \ xor edi, edi (2)
  $0f c, $05 c, ;                   \ syscall (2)

: write-out
  s" mandelbrot" w/o create-file throw >r
  elf-buf 120 r@ write-file throw
  code-buf code-pos @ r@ write-file throw
  r> close-file throw ;

gen-code
elf-header
write-out
s" chmod +x mandelbrot" system drop
bye
