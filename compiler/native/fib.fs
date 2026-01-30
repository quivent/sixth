\ fib.fs - Compute Fibonacci(40) = 102334155
\ Iterative algorithm

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

\ fib(40) iterative: a=0, b=1, repeat 40 times: (a,b) = (b, a+b)
\ rbx=a, rcx=b, r8=counter
: gen-code
  0 code-pos !
  \ 0: xor ebx, ebx (a=0)
  $31 c, $db c,
  \ 2: mov ecx, 1 (b=1)
  $b9 c, 1 d,
  \ 7: mov r8d, 40 (counter)
  $41 c, $b8 c, 40 d,
  \ loop @ 13:
  \ 13: mov eax, ebx (temp = a)
  $89 c, $d8 c,
  \ 15: mov ebx, ecx (a = b)
  $89 c, $cb c,
  \ 17: add ecx, eax (b = b + temp)
  $01 c, $c1 c,
  \ 19: dec r8d
  $41 c, $ff c, $c8 c,
  \ 22: jnz loop (-11 = 0xf5)
  $75 c, $f5 c,
  \ 24: mov eax, ebx (result in eax)
  $89 c, $d8 c,
  \ Print
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
  s" fib" w/o create-file throw >r
  elf-buf 120 r@ write-file throw
  code-buf code-pos @ r@ write-file throw
  r> close-file throw ;

gen-code
elf-header
write-out
s" chmod +x fib" system drop
bye
