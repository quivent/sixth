\ qsort.fs - Bubble sort demo
\ Sorts 16 hardcoded values and prints them
\ Expected: 1 2 3 5 8 11 12 17 22 25 33 42 47 64 91 99

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

\ Byte offsets (calculate as we go):

: gen-code
  0 code-pos !

  \ 0: mmap 64 bytes for array
  $31 c, $ff c,                     \ xor edi, edi (2)
  $be c, 64 d,                      \ mov esi, 64 (5)
  $ba c, 3 d,                       \ mov edx, 3 (5)
  $41 c, $ba c, $22 d,              \ mov r10d, 0x22 (6)
  $49 c, $c7 c, $c0 c, $ff c, $ff c, $ff c, $ff c,  \ mov r8, -1 (7)
  $45 c, $31 c, $c9 c,              \ xor r9d, r9d (3)
  $b8 c, 9 d,                       \ mov eax, 9 (5)
  $0f c, $05 c,                     \ syscall (2) -> 35
  $49 c, $89 c, $c4 c,              \ mov r12, rax (3) -> 38

  \ 38: Initialize array (each mov is 8 bytes: 41 c7 44 24 offset imm32)
  \ First one is 7 bytes: 41 c7 04 24 imm32
  $41 c, $c7 c, $04 c, $24 c, 64 d,          \ [r12+0]=64 (7) -> 45
  $41 c, $c7 c, $44 c, $24 c, 4 c, 25 d,     \ [r12+4]=25 (8) -> 53
  $41 c, $c7 c, $44 c, $24 c, 8 c, 12 d,     \ 61
  $41 c, $c7 c, $44 c, $24 c, 12 c, 22 d,    \ 69
  $41 c, $c7 c, $44 c, $24 c, 16 c, 11 d,    \ 77
  $41 c, $c7 c, $44 c, $24 c, 20 c, 1 d,     \ 85
  $41 c, $c7 c, $44 c, $24 c, 24 c, 99 d,    \ 93
  $41 c, $c7 c, $44 c, $24 c, 28 c, 3 d,     \ 101
  $41 c, $c7 c, $44 c, $24 c, 32 c, 47 d,    \ 109
  $41 c, $c7 c, $44 c, $24 c, 36 c, 8 d,     \ 117
  $41 c, $c7 c, $44 c, $24 c, 40 c, 91 d,    \ 125
  $41 c, $c7 c, $44 c, $24 c, 44 c, 5 d,     \ 133
  $41 c, $c7 c, $44 c, $24 c, 48 c, 17 d,    \ 141
  $41 c, $c7 c, $44 c, $24 c, 52 c, 33 d,    \ 149
  $41 c, $c7 c, $44 c, $24 c, 56 c, 2 d,     \ 157
  $41 c, $c7 c, $44 c, $24 c, 60 c, 42 d,    \ 165

  \ 165: Bubble sort
  \ r13 = outer (0 to 14)
  \ r14 = inner (0 to 14-outer)
  $45 c, $31 c, $ed c,              \ xor r13d, r13d (3) -> 168

  \ 168: outer_loop
  $45 c, $31 c, $f6 c,              \ xor r14d, r14d (j=0) (3) -> 171

  \ 171: inner_loop
  $44 c, $89 c, $f0 c,              \ mov eax, r14d (3) -> 174
  $c1 c, $e0 c, 2 c,                \ shl eax, 2 (3) -> 177
  $41 c, $8b c, $0c c, $04 c,       \ mov ecx, [r12+rax] (4) -> 181
  $41 c, $8b c, $54 c, $04 c, 4 c,  \ mov edx, [r12+rax+4] (5) -> 186
  $39 c, $d1 c,                     \ cmp ecx, edx (2) -> 188
  $7e c, 9 c,                       \ jle no_swap (2) -> 190, target 199

  \ 190: swap
  $41 c, $89 c, $14 c, $04 c,       \ mov [r12+rax], edx (4) -> 194
  $41 c, $89 c, $4c c, $04 c, 4 c,  \ mov [r12+rax+4], ecx (5) -> 199

  \ 199: no_swap
  $41 c, $ff c, $c6 c,              \ inc r14d (3) -> 202
  \ Compare: r14 < 15 - r13
  $41 c, $89 c, $ef c,              \ mov r15d, r13d (3) -> 205
  $41 c, $f7 c, $d7 c,              \ not r15d (3) -> 208
  $41 c, $83 c, $c7 c, 16 c,        \ add r15d, 16 (gives 15-r13) (4) -> 212
  $45 c, $39 c, $fe c,              \ cmp r14d, r15d (3) -> 215
  $7c c, $d2 c,                     \ jl inner_loop (2) -> -46 = 0xd2

  \ 217: end inner
  $41 c, $ff c, $c5 c,              \ inc r13d (3) -> 220
  $41 c, $83 c, $fd c, 15 c,        \ cmp r13d, 15 (4) -> 224
  $7c c, $c6 c,                     \ jl outer_loop (2) -> -58 = 0xc6

  \ 226: Print sorted array
  $45 c, $31 c, $ed c,              \ xor r13d, r13d (3) -> 229

  \ 229: print_loop
  $44 c, $89 c, $e8 c,              \ mov eax, r13d (3) -> 232
  $c1 c, $e0 c, 2 c,                \ shl eax, 2 (3) -> 235
  $41 c, $8b c, $04 c, $04 c,       \ mov eax, [r12+rax] (4) -> 239
  $b9 c, 10 d,                      \ mov ecx, 10 (5) -> 244
  $45 c, $31 c, $f6 c,              \ xor r14d, r14d (3) -> 247

  \ 247: digit_loop
  $31 c, $d2 c,                     \ xor edx, edx (2) -> 249
  $f7 c, $f1 c,                     \ div ecx (2) -> 251
  $83 c, $c2 c, $30 c,              \ add edx, '0' (3) -> 254
  $52 c,                            \ push rdx (1) -> 255
  $41 c, $ff c, $c6 c,              \ inc r14d (3) -> 258
  $85 c, $c0 c,                     \ test eax, eax (2) -> 260
  $75 c, $f1 c,                     \ jnz digit_loop (2) -> 262, target 247 = -15

  \ 262: output_loop
  $b8 c, 1 d,                       \ mov eax, 1 (5) -> 267
  $bf c, 1 d,                       \ mov edi, 1 (5) -> 272
  $48 c, $89 c, $e6 c,              \ mov rsi, rsp (3) -> 275
  $ba c, 1 d,                       \ mov edx, 1 (5) -> 280
  $0f c, $05 c,                     \ syscall (2) -> 282
  $58 c,                            \ pop rax (1) -> 283
  $41 c, $ff c, $ce c,              \ dec r14d (3) -> 286
  $75 c, $e6 c,                     \ jnz output_loop (2) -> -26 = 0xe6

  \ 288: Print space
  $6a c, 32 c,                      \ push 32 (2) -> 290
  $b8 c, 1 d,                       \ mov eax, 1 (5) -> 295
  $bf c, 1 d,                       \ mov edi, 1 (5) -> 300
  $48 c, $89 c, $e6 c,              \ mov rsi, rsp (3) -> 303
  $ba c, 1 d,                       \ mov edx, 1 (5) -> 308
  $0f c, $05 c,                     \ syscall (2) -> 310
  $58 c,                            \ pop rax (1) -> 311

  \ 311: next print
  $41 c, $ff c, $c5 c,              \ inc r13d (3) -> 314
  $41 c, $83 c, $fd c, 16 c,        \ cmp r13d, 16 (4) -> 318
  $7c c, $a5 c,                     \ jl print_loop (2) -> -91 = 0xa5

  \ 320: Newline
  $6a c, 10 c,                      \ push 10 (2) -> 322
  $b8 c, 1 d,                       \ mov eax, 1 (5) -> 327
  $bf c, 1 d,                       \ mov edi, 1 (5) -> 332
  $48 c, $89 c, $e6 c,              \ mov rsi, rsp (3) -> 335
  $ba c, 1 d,                       \ mov edx, 1 (5) -> 340
  $0f c, $05 c,                     \ syscall (2) -> 342
  $58 c,                            \ pop rax (1) -> 343

  \ 343: Exit
  $b8 c, 60 d,                      \ mov eax, 60 (5) -> 348
  $31 c, $ff c,                     \ xor edi, edi (2) -> 350
  $0f c, $05 c, ;                   \ syscall (2) -> 352

: write-out
  s" qsort" w/o create-file throw >r
  elf-buf 120 r@ write-file throw
  code-buf code-pos @ r@ write-file throw
  r> close-file throw ;

gen-code
elf-header
write-out
s" chmod +x qsort" system drop
bye
