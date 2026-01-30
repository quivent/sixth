\ fib45.fs - Recursive Fibonacci(45)
\ Result: 1134903170
\ Tests function call overhead (1.1 billion calls)

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

\ fib(n) recursive: if n<2 return n, else fib(n-1)+fib(n-2)
\ edi = n, eax = result
: gen-code
  0 code-pos !
  \ 0: mov edi, 45 (5)
  $bf c, 45 d,
  \ 5: call fib@12 (5) offset = 12-10 = 2
  $e8 c, 2 d,
  \ 10: jmp print@47 (2) offset = 47-12 = 35
  $eb c, 35 c,

  \ fib @ 12
  \ 12: cmp edi, 2 (3)
  $83 c, $ff c, 2 c,
  \ 15: jl base_case@43 (2) offset = 43-17 = 26
  $7c c, 26 c,
  \ 17: push rdi (1)
  $57 c,
  \ 18: dec edi (2)
  $ff c, $cf c,
  \ 20: call fib@12 (5) offset = 12-25 = -13
  $e8 c, $f3 c, $ff c, $ff c, $ff c,
  \ 25: push rax (1)
  $50 c,
  \ 26: pop rdi (1) -- restore n
  $5f c,
  \ 27: sub edi, 2 (3)
  $83 c, $ef c, 2 c,
  \ 30: call fib@12 (5) offset = 12-35 = -23
  $e8 c, $e9 c, $ff c, $ff c, $ff c,
  \ 35: pop rbx (1) -- fib(n-1)
  $5b c,
  \ 36: add eax, ebx (2)
  $01 c, $d8 c,
  \ 38: ret (1)
  $c3 c,

  \ base_case @ 39 (not 43!)
  \ Let me recalculate...
  \ Actually jl offset should be to base_case
  \ After jl @17, base_case is at current pos
  \ 39: mov eax, edi (2)
  $89 c, $f8 c,
  \ 41: ret (1)
  $c3 c,

  \ Hmm, I had the base_case offset wrong. Let me redo.
  \ After the ret at 38, base_case is at 39.
  \ jl at byte 15-17, jumps to 39. offset = 39-17 = 22
  \ Let me also recalc: jmp to print. After ret@38 and base_case...
  \ base_case ends at 42. print starts at 42.
  \ jmp@10-12 needs to jump to 42. offset = 42-12 = 30

  \ I'll redo this more carefully with actual byte tracking.
  ;

\ This is getting error-prone. Let me use a simpler approach.
\ I'll track offsets with variables.

variable fib-addr
variable print-addr

: gen-code-v2
  0 code-pos !
  \ Entry
  $bf c, 45 d,                  \ mov edi, 45
  $e8 c, 0 d, code-pos @ 4 - >r \ call fib (placeholder)
  $eb c, 0 c, code-pos @ 1 - >r \ jmp print (placeholder)

  \ fib function
  code-pos @ fib-addr !
  $83 c, $ff c, 2 c,            \ cmp edi, 2
  $7c c, 0 c, code-pos @ 1 - >r \ jl base_case (placeholder)
  $57 c,                        \ push rdi
  $ff c, $cf c,                 \ dec edi
  $e8 c, 0 d, code-pos @ 4 - >r \ call fib (placeholder)
  $50 c,                        \ push rax
  $5f c,                        \ pop rdi
  $83 c, $ef c, 2 c,            \ sub edi, 2
  $e8 c, 0 d, code-pos @ 4 - >r \ call fib (placeholder)
  $5b c,                        \ pop rbx
  $01 c, $d8 c,                 \ add eax, ebx
  $c3 c,                        \ ret
  \ base_case
  code-pos @ >r                 \ save base_case addr
  $89 c, $f8 c,                 \ mov eax, edi
  $c3 c,                        \ ret

  \ Now patch. This is getting complex. Let me just hardcode it right.
  ;

\ Forget the variable approach. Hardcode with correct offsets:
: gen-code
  0 code-pos !
  \ 0: mov edi, 45 (5 bytes)
  $bf c, 45 d,
  \ 5: call fib (5 bytes) - fib is at 12, from 10: offset=2
  $e8 c, 2 d,
  \ 10: jmp print (2 bytes) - print at 42, from 12: offset=30
  $eb c, 30 c,

  \ 12: fib:
  \ 12: cmp edi, 2 (3)
  $83 c, $ff c, 2 c,
  \ 15: jl base (2) - base at 39, from 17: offset=22
  $7c c, 22 c,
  \ 17: push rdi (1)
  $57 c,
  \ 18: dec edi (2)
  $ff c, $cf c,
  \ 20: call fib (5) - fib at 12, from 25: offset=-13
  $e8 c, $f3 c, $ff c, $ff c, $ff c,
  \ 25: push rax (1)
  $50 c,
  \ 26: pop rdi (1)
  $5f c,
  \ 27: sub edi, 2 (3)
  $83 c, $ef c, 2 c,
  \ 30: call fib (5) - fib at 12, from 35: offset=-23
  $e8 c, $e9 c, $ff c, $ff c, $ff c,
  \ 35: pop rbx (1)
  $5b c,
  \ 36: add eax, ebx (2)
  $01 c, $d8 c,
  \ 38: ret (1)
  $c3 c,
  \ 39: base: mov eax, edi (2)
  $89 c, $f8 c,
  \ 41: ret (1)
  $c3 c,

  \ 42: print:
  $b9 c, 10 d,                  \ mov ecx, 10
  $45 c, $31 c, $c0 c,          \ xor r8d, r8d
  \ digit_loop @ 50:
  $31 c, $d2 c,                 \ xor edx, edx (2)
  $f7 c, $f1 c,                 \ div ecx (2)
  $83 c, $c2 c, $30 c,          \ add edx, '0' (3)
  $52 c,                        \ push rdx (1)
  $41 c, $ff c, $c0 c,          \ inc r8d (3)
  $85 c, $c0 c,                 \ test eax, eax (2)
  \ jnz digit_loop: from 65 to 50 = -15 = 0xf1
  $75 c, $f1 c,
  \ print_loop @ 67:
  $b8 c, 1 d,                   \ mov eax, 1 (5)
  $bf c, 1 d,                   \ mov edi, 1 (5)
  $48 c, $89 c, $e6 c,          \ mov rsi, rsp (3)
  $ba c, 1 d,                   \ mov edx, 1 (5)
  $0f c, $05 c,                 \ syscall (2)
  $58 c,                        \ pop (1)
  $41 c, $ff c, $c8 c,          \ dec r8d (3)
  \ jnz print_loop: from 93 to 67 = -26 = 0xe6
  $75 c, $e6 c,
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
  s" fib45" w/o create-file throw >r
  elf-buf 120 r@ write-file throw
  code-buf code-pos @ r@ write-file throw
  r> close-file throw ;

gen-code
elf-header
write-out
s" chmod +x fib45" system drop
bye
