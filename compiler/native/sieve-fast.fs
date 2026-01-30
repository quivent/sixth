\ sieve-fast.fs - Sieve with SSE2 counting
\ Count primes up to 1,000,000 = 78498

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

: gen-code
  0 code-pos !
  \ === MMAP (0-38) - unchanged ===
  $31 c, $ff c,
  $be c, 1000001 d,
  $ba c, 3 d,
  $41 c, $ba c, $22 d,
  $49 c, $c7 c, $c0 c, $ff c, $ff c, $ff c, $ff c,
  $45 c, $31 c, $c9 c,
  $b8 c, 9 d,
  $0f c, $05 c,
  $49 c, $89 c, $c4 c,
  $41 c, $bd c, 2 d,

  \ === MARKING (44-97) - unchanged ===
  \ outer @ 44
  $43 c, $8a c, $0c c, $2c c,
  $84 c, $c9 c,
  $75 c, 33 c,                      \ jnz next_i
  $44 c, $89 c, $e8 c,
  $0f c, $af c, $c0 c,
  $3d c, 1000001 d,
  $7d c, 32 c,                      \ jge count
  $41 c, $89 c, $c6 c,
  \ inner @ 68
  $43 c, $c6 c, $04 c, $34 c, 1 c,
  $45 c, $01 c, $ee c,
  $41 c, $81 c, $fe c, 1000001 d,
  $7c c, $ef c,                     \ jl inner
  \ next_i @ 85
  $41 c, $ff c, $c5 c,
  $41 c, $81 c, $fd c, 1001 d,
  $7c c, $cb c,                     \ jl outer

  \ === SSE2 COUNTING (97-169) ===
  \ count @ 97
  $45 c, $31 c, $ff c,              \ xor r15d, r15d (3) -> 100
  $41 c, $bd c, 2 d,                \ mov r13d, 2 (6) -> 106
  $66 c, $0f c, $ef c, $c9 c,       \ pxor xmm1, xmm1 (4) -> 110
  \ count_simd @ 110
  $f3 c, $43 c, $0f c, $6f c, $04 c, $2c c,  \ movdqu xmm0, [r12+r13] (6) -> 116
  $66 c, $0f c, $74 c, $c1 c,       \ pcmpeqb xmm0, xmm1 (4) -> 120
  $66 c, $0f c, $d7 c, $c0 c,       \ pmovmskb eax, xmm0 (4) -> 124
  $f3 c, $0f c, $b8 c, $c0 c,       \ popcnt eax, eax (4) -> 128
  $41 c, $01 c, $c7 c,              \ add r15d, eax (3) -> 131
  $41 c, $83 c, $c5 c, 16 c,        \ add r13d, 16 (4) -> 135
  $41 c, $81 c, $fd c, 999986 d,    \ cmp r13d, 999986 (7) -> 142
  $7c c, $de c,                     \ jl count_simd (2) -> 144, -34
  \ count_cleanup @ 144
  $41 c, $81 c, $fd c, 1000001 d,   \ cmp r13d, 1000001 (7) -> 151
  $7d c, 16 c,                      \ jge print (2) -> 153, +16 to 169
  $43 c, $8a c, $04 c, $2c c,       \ mov al, [r12+r13] (4) -> 157
  $84 c, $c0 c,                     \ test al, al (2) -> 159
  $75 c, 3 c,                       \ jnz skip_inc2 (2) -> 161
  $41 c, $ff c, $c7 c,              \ inc r15d (3) -> 164
  \ skip_inc2 @ 164
  $41 c, $ff c, $c5 c,              \ inc r13d (3) -> 167
  $eb c, $e7 c,                     \ jmp count_cleanup (2) -> 169, -25

  \ === PRINT (169-...) ===
  \ print @ 169
  $44 c, $89 c, $f8 c,              \ mov eax, r15d
  $b9 c, 10 d,
  $45 c, $31 c, $c0 c,
  \ digit_loop @ 180
  $31 c, $d2 c,
  $f7 c, $f1 c,
  $83 c, $c2 c, $30 c,
  $52 c,
  $41 c, $ff c, $c0 c,
  $85 c, $c0 c,
  $75 c, $f1 c,                     \ jnz digit_loop
  \ print_loop @ 195
  $b8 c, 1 d,
  $bf c, 1 d,
  $48 c, $89 c, $e6 c,
  $ba c, 1 d,
  $0f c, $05 c,
  $58 c,
  $41 c, $ff c, $c8 c,
  $75 c, $e6 c,                     \ jnz print_loop
  \ newline
  $6a c, 10 c,
  $b8 c, 1 d,
  $bf c, 1 d,
  $48 c, $89 c, $e6 c,
  $ba c, 1 d,
  $0f c, $05 c,
  $58 c,
  \ exit
  $b8 c, 60 d,
  $31 c, $ff c,
  $0f c, $05 c, ;

: write-out
  s" compiler/native/sieve-fast" w/o create-file throw >r
  elf-buf 120 r@ write-file throw
  code-buf code-pos @ r@ write-file throw
  r> close-file throw ;

gen-code
elf-header
write-out
s" chmod +x compiler/native/sieve-fast" system drop
bye
