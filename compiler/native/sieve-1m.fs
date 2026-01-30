\ sieve-1m.fs - Sieve of Eratosthenes using mmap
\ Count primes up to 1,000,000
\ Result: 78498 primes

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
  \ mmap(0, 1000001, 3, 0x22, -1, 0)
  $31 c, $ff c,                     \ xor edi, edi
  $be c, 1000001 d,                 \ mov esi, 1000001
  $ba c, 3 d,                       \ mov edx, 3
  $41 c, $ba c, $22 d,              \ mov r10d, 0x22
  $49 c, $c7 c, $c0 c, $ff c, $ff c, $ff c, $ff c,  \ mov r8, -1
  $45 c, $31 c, $c9 c,              \ xor r9d, r9d
  $b8 c, 9 d,                       \ mov eax, 9
  $0f c, $05 c,                     \ syscall
  $49 c, $89 c, $c4 c,              \ mov r12, rax (sieve base)
  $41 c, $bd c, 2 d,                \ mov r13d, 2 (i = 2)
  \ outer @ 44
  $43 c, $8a c, $0c c, $2c c,       \ mov cl, [r12+r13]
  $84 c, $c9 c,                     \ test cl, cl
  $75 c, 33 c,                      \ jnz next_i (+33 to offset 85)
  \ i*i - need 64-bit for large values
  $44 c, $89 c, $e8 c,              \ mov eax, r13d
  $0f c, $af c, $c0 c,              \ imul eax, eax
  $3d c, 1000001 d,                 \ cmp eax, 1000001
  $7d c, 32 c,                      \ jge count (+32 to offset 97)
  $41 c, $89 c, $c6 c,              \ mov r14d, eax (j = i*i)
  \ inner @ 68
  $43 c, $c6 c, $04 c, $34 c, 1 c,  \ mov byte [r12+r14], 1
  $45 c, $01 c, $ee c,              \ add r14d, r13d
  $41 c, $81 c, $fe c, 1000001 d,   \ cmp r14d, 1000001
  $7c c, $ef c,                     \ jl inner (-17 to offset 68)
  \ next_i @ 87
  $41 c, $ff c, $c5 c,              \ inc r13d
  $41 c, $81 c, $fd c, 1001 d,      \ cmp r13d, 1001 (sqrt(1000000))
  $7c c, $cb c,                     \ jl outer (-53 to offset 44)
  \ count @ 99
  $45 c, $31 c, $ff c,              \ xor r15d, r15d
  $41 c, $bd c, 2 d,                \ mov r13d, 2
  \ count_loop @ 108
  $43 c, $8a c, $04 c, $2c c,       \ mov al, [r12+r13]
  $84 c, $c0 c,                     \ test al, al
  $75 c, 3 c,                       \ jnz skip_inc
  $41 c, $ff c, $c7 c,              \ inc r15d
  \ skip_inc @ 119
  $41 c, $ff c, $c5 c,              \ inc r13d
  $41 c, $81 c, $fd c, 1000001 d,   \ cmp r13d, 1000001
  $7c c, $e9 c,                     \ jl count_loop (-23 to offset 106)
  \ print r15
  $44 c, $89 c, $f8 c,              \ mov eax, r15d
  $b9 c, 10 d,                      \ mov ecx, 10
  $45 c, $31 c, $c0 c,              \ xor r8d, r8d (digit count)
  \ digit_loop
  $31 c, $d2 c,                     \ xor edx, edx
  $f7 c, $f1 c,                     \ div ecx
  $83 c, $c2 c, $30 c,              \ add edx, '0'
  $52 c,                            \ push rdx
  $41 c, $ff c, $c0 c,              \ inc r8d
  $85 c, $c0 c,                     \ test eax, eax
  $75 c, $f1 c,                     \ jnz digit_loop
  \ print_loop
  $b8 c, 1 d,                       \ mov eax, 1
  $bf c, 1 d,                       \ mov edi, 1
  $48 c, $89 c, $e6 c,              \ mov rsi, rsp
  $ba c, 1 d,                       \ mov edx, 1
  $0f c, $05 c,                     \ syscall
  $58 c,                            \ pop rax
  $41 c, $ff c, $c8 c,              \ dec r8d
  $75 c, $e6 c,                     \ jnz print_loop
  \ newline
  $6a c, 10 c,                      \ push 10
  $b8 c, 1 d,                       \ mov eax, 1
  $bf c, 1 d,                       \ mov edi, 1
  $48 c, $89 c, $e6 c,              \ mov rsi, rsp
  $ba c, 1 d,                       \ mov edx, 1
  $0f c, $05 c,                     \ syscall
  $58 c,                            \ pop rax
  \ exit
  $b8 c, 60 d,                      \ mov eax, 60
  $31 c, $ff c,                     \ xor edi, edi
  $0f c, $05 c, ;                   \ syscall

: write-out
  s" sieve-1m" w/o create-file throw >r
  elf-buf 120 r@ write-file throw
  code-buf code-pos @ r@ write-file throw
  r> close-file throw ;

gen-code
elf-header
write-out
s" chmod +x sieve-1m" system drop
bye
