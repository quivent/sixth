\ fib-rec.fs - Recursive Fibonacci with call/ret
\ fib(35) = 9227465

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
  \ 0: mov edi, 35
  $bf c, 35 d,
  \ 5: call fib @ 91 (offset = 91-10 = 81)
  $e8 c, 81 d,
  \ 10: print routine (81 bytes to offset 91)
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
  $0f c, $05 c,
  \ fib @ 91
  \ 91: cmp edi, 2
  $83 c, $ff c, 2 c,
  \ 94: jl base_case @ 120, offset=24
  $7c c, 24 c,
  \ 96: push rbx
  $53 c,
  \ 97: push rdi
  $57 c,
  \ 98: dec edi
  $ff c, $cf c,
  \ 100: call fib @ 91 (offset = 91-105 = -14)
  $e8 c, $f2 c, $ff c, $ff c, $ff c,
  \ 105: mov ebx, eax
  $89 c, $c3 c,
  \ 107: pop rdi
  $5f c,
  \ 108: sub edi, 2
  $83 c, $ef c, 2 c,
  \ 111: call fib @ 91 (offset = 91-116 = -25)
  $e8 c, $e7 c, $ff c, $ff c, $ff c,
  \ 116: add eax, ebx
  $01 c, $d8 c,
  \ 118: pop rbx
  $5b c,
  \ 119: ret
  $c3 c,
  \ 120: base_case
  $89 c, $f8 c,
  \ 122: ret
  $c3 c, ;

: write-out
  s" fib-rec" w/o create-file throw >r
  elf-buf 120 r@ write-file throw
  code-buf code-pos @ r@ write-file throw
  r> close-file throw ;

gen-code
elf-header
write-out
s" chmod +x fib-rec" system drop
bye
