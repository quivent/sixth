\ primes.fs - Count primes up to 1,000,000
\ Result: 78498 primes
\ Demonstrates: nested loops, modulo, conditionals
\ Performance: matches C -O2

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

\ r12=count, r13=n, r14=divisor
: gen-code
  0 code-pos !
  \ 0: xor r12d, r12d (count=0)
  $45 c, $31 c, $e4 c,
  \ 3: mov r13d, 2 (n=2)
  $41 c, $bd c, 2 d,
  \ 9: mov r14d, 2 (divisor=2)
  $41 c, $be c, 2 d,
  \ 15: cmp r14d, r13d
  $45 c, $39 c, $ee c,
  \ 18: jge is_prime (+17)
  $7d c, 17 c,
  \ 20: mov eax, r13d
  $44 c, $89 c, $e8 c,
  \ 23: xor edx, edx
  $31 c, $d2 c,
  \ 25: div r14d
  $41 c, $f7 c, $f6 c,
  \ 28: test edx, edx
  $85 c, $d2 c,
  \ 30: jz next_n (+8)
  $74 c, 8 c,
  \ 32: inc r14d
  $41 c, $ff c, $c6 c,
  \ 35: jmp inner (-22 = 0xea)
  $eb c, $ea c,
  \ 37: inc r12d
  $41 c, $ff c, $c4 c,
  \ 40: inc r13d
  $41 c, $ff c, $c5 c,
  \ 43: cmp r13d, 1000001
  $41 c, $81 c, $fd c, 1000001 d,
  \ 50: jl outer (-43 = 0xd5)
  $7c c, $d5 c,
  \ 52: mov eax, r12d
  $44 c, $89 c, $e0 c,
  \ Print number
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
  s" primes" w/o create-file throw >r
  elf-buf 120 r@ write-file throw
  code-buf code-pos @ r@ write-file throw
  r> close-file throw ;

gen-code
elf-header
write-out
s" chmod +x primes" system drop
bye
