\ sieve.fs - Sieve of Eratosthenes using mmap
\ Count primes up to 10,000
\ Result: 1229 primes

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
  \ mmap
  $31 c, $ff c,
  $be c, 10001 d,
  $ba c, 3 d,
  $41 c, $ba c, $22 d,
  $49 c, $c7 c, $c0 c, $ff c, $ff c, $ff c, $ff c,
  $45 c, $31 c, $c9 c,
  $b8 c, 9 d,
  $0f c, $05 c,
  $49 c, $89 c, $c4 c,
  $41 c, $bd c, 2 d,
  \ outer @ 44
  \ check if sieve[i] is composite first (use cl to not clobber eax)
  $43 c, $8a c, $0c c, $2c c,       \ mov cl, [r12+r13]
  $84 c, $c9 c,                     \ test cl, cl
  $75 c, 33 c,                      \ jnz next_i (+33 to offset 85)
  \ i*i
  $44 c, $89 c, $e8 c,              \ mov eax, r13d
  $0f c, $af c, $c0 c,              \ imul eax, eax
  $3d c, 10001 d,                   \ cmp eax, 10001
  $7d c, 32 c,                      \ jge count (+32 to offset 97)
  $41 c, $89 c, $c6 c,              \ mov r14d, eax
  \ inner
  $43 c, $c6 c, $04 c, $34 c, 1 c,  \ mov byte [r12+r14], 1
  $45 c, $01 c, $ee c,              \ add r14d, r13d
  $41 c, $81 c, $fe c, 10001 d,     \ cmp r14d, 10001
  $7c c, $ef c,                     \ jl inner
  \ next_i
  $41 c, $ff c, $c5 c,              \ inc r13d
  $41 c, $81 c, $fd c, 101 d,       \ cmp r13d, 101
  $7c c, $cb c,                     \ jl outer (-53 to offset 44)
  \ count
  $45 c, $31 c, $ff c,
  $41 c, $bd c, 2 d,
  \ count_loop
  $43 c, $8a c, $04 c, $2c c,
  $84 c, $c0 c,
  $75 c, 3 c,
  $41 c, $ff c, $c7 c,
  $41 c, $ff c, $c5 c,
  $41 c, $81 c, $fd c, 10001 d,
  $7c c, $e9 c,                     \ jl count_loop (-23 to offset 106)
  \ print
  $44 c, $89 c, $f8 c,
  $b9 c, 10 d,
  $45 c, $31 c, $c0 c,
  $31 c, $d2 c,
  $f7 c, $f1 c,
  $83 c, $c2 c, $30 c,
  $52 c,
  $41 c, $ff c, $c0 c,
  $85 c, $c0 c,
  $75 c, $f1 c,
  $b8 c, 1 d,
  $bf c, 1 d,
  $48 c, $89 c, $e6 c,
  $ba c, 1 d,
  $0f c, $05 c,
  $58 c,
  $41 c, $ff c, $c8 c,
  $75 c, $e6 c,
  $6a c, 10 c,
  $b8 c, 1 d,
  $bf c, 1 d,
  $48 c, $89 c, $e6 c,
  $ba c, 1 d,
  $0f c, $05 c,
  $58 c,
  $b8 c, 60 d,
  $31 c, $ff c,
  $0f c, $05 c, ;

: write-out
  s" sieve" w/o create-file throw >r
  elf-buf 120 r@ write-file throw
  code-buf code-pos @ r@ write-file throw
  r> close-file throw ;

gen-code
elf-header
write-out
s" chmod +x sieve" system drop
bye
