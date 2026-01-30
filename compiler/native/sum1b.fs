\ sum1b.fs - Sum 1 to 1,000,000,000
\ Result: 500000000500000000

create code-buf 4096 allot
variable code-pos  0 code-pos !
create elf-buf 256 allot
variable elf-pos  0 elf-pos !

: c, code-buf code-pos @ + c!  1 code-pos +! ;
: d, dup c, 8 rshift dup c, 8 rshift dup c, 8 rshift c, ;
: q, dup d, 32 rshift d, ;
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

\ Gauss: n*(n+1)/2 instead of loop
: gen-code
  0 code-pos !
  \ mov rax, 1000000000 (10 bytes)
  $48 c, $b8 c, 1000000000 q,
  \ mov rbx, 1000000001 (10 bytes)
  $48 c, $bb c, 1000000001 q,
  \ imul rax, rbx (4 bytes)
  $48 c, $0f c, $af c, $c3 c,
  \ shr rax, 1 (3 bytes)
  $48 c, $d1 c, $e8 c,
  \ 25: mov ecx, 10 (5)
  $b9 c, 10 d,
  \ 30: xor r8d, r8d (3)
  $45 c, $31 c, $c0 c,
  \ 33: digit_loop: xor edx, edx (2)
  $31 c, $d2 c,
  \ 35: div rcx (3)
  $48 c, $f7 c, $f1 c,
  \ 38: add edx, '0' (3)
  $83 c, $c2 c, $30 c,
  \ 41: push rdx (1)
  $52 c,
  \ 42: inc r8d (3)
  $41 c, $ff c, $c0 c,
  \ 45: test rax, rax (3)
  $48 c, $85 c, $c0 c,
  \ 48: jnz digit_loop (2) -> @33, from 50: -17 = 0xef
  $75 c, $ef c,
  \ 50: print_loop: mov eax, 1 (5)
  $b8 c, 1 d,
  \ 55: mov edi, 1 (5)
  $bf c, 1 d,
  \ 60: mov rsi, rsp (3)
  $48 c, $89 c, $e6 c,
  \ 63: mov edx, 1 (5)
  $ba c, 1 d,
  \ 68: syscall (2)
  $0f c, $05 c,
  \ 70: pop rax (1)
  $58 c,
  \ 71: dec r8d (3)
  $41 c, $ff c, $c8 c,
  \ 74: jnz print_loop (2) -> @50, from 76: -26 = 0xe6
  $75 c, $e6 c,
  \ 76: push 10 (2)
  $6a c, 10 c,
  \ 78: write newline
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
  s" sum1b" w/o create-file throw >r
  elf-buf 120 r@ write-file throw
  code-buf code-pos @ r@ write-file throw
  r> close-file throw ;

gen-code
elf-header
write-out
s" chmod +x sum1b" system drop
bye
