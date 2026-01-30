\ sum.fs - Sum 1 to 1,000,000
\ Result: 500000500000

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

\ rbx=sum, rcx=counter
: gen-code
  0 code-pos !
  \ 0: xor rbx, rbx
  $48 c, $31 c, $db c,
  \ 3: mov ecx, 1
  $b9 c, 1 d,
  \ 8: add rbx, rcx
  $48 c, $01 c, $cb c,
  \ 11: inc ecx
  $ff c, $c1 c,
  \ 13: cmp ecx, 1000001
  $81 c, $f9 c, 1000001 d,
  \ 19: jl loop (8, from 21 = -13 = 0xf3)
  $7c c, $f3 c,
  \ 21: mov rax, rbx
  $48 c, $89 c, $d8 c,
  \ 24: mov ecx, 10
  $b9 c, 10 d,
  \ 29: xor r8d, r8d
  $45 c, $31 c, $c0 c,
  \ digit_loop @ 32:
  \ 32: xor edx, edx
  $31 c, $d2 c,
  \ 34: div rcx
  $48 c, $f7 c, $f1 c,
  \ 37: add edx, '0'
  $83 c, $c2 c, $30 c,
  \ 40: push rdx
  $52 c,
  \ 41: inc r8d
  $41 c, $ff c, $c0 c,
  \ 44: test rax, rax
  $48 c, $85 c, $c0 c,
  \ 47: jnz digit_loop (32, from 49 = -17 = 0xef)
  $75 c, $ef c,
  \ print_loop @ 49:
  \ 49: mov eax, 1
  $b8 c, 1 d,
  \ 54: mov edi, 1
  $bf c, 1 d,
  \ 59: mov rsi, rsp
  $48 c, $89 c, $e6 c,
  \ 62: mov edx, 1
  $ba c, 1 d,
  \ 67: syscall
  $0f c, $05 c,
  \ 69: pop rax
  $58 c,
  \ 70: dec r8d
  $41 c, $ff c, $c8 c,
  \ 73: jnz print_loop (49, from 75 = -26 = 0xe6)
  $75 c, $e6 c,
  \ 75: push 10
  $6a c, 10 c,
  \ 77: mov eax, 1
  $b8 c, 1 d,
  \ 82: mov edi, 1
  $bf c, 1 d,
  \ 87: mov rsi, rsp
  $48 c, $89 c, $e6 c,
  \ 90: mov edx, 1
  $ba c, 1 d,
  \ 95: syscall
  $0f c, $05 c,
  \ 97: pop rax
  $58 c,
  \ 98: mov eax, 60
  $b8 c, 60 d,
  \ 103: xor edi, edi
  $31 c, $ff c,
  \ 105: syscall
  $0f c, $05 c, ;

: write-out
  s" sum" w/o create-file throw >r
  elf-buf 120 r@ write-file throw
  code-buf code-pos @ r@ write-file throw
  r> close-file throw ;

gen-code
elf-header
write-out
s" chmod +x sum" system drop
bye
