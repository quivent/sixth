\ loop10b.fs - 10 billion empty loop iterations
\ Tests pure loop overhead

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

\ rcx = counter (10 billion)
: gen-code
  0 code-pos !
  \ 0: mov rcx, 10000000000 (10 bytes)
  $48 c, $b9 c, 10000000000 q,
  \ 10: loop: dec rcx (3 bytes)
  $48 c, $ff c, $c9 c,
  \ 13: jnz loop (-5)
  $75 c, $fb c,
  \ 15: jmp +5 to skip string
  $eb c, 5 c,
  \ 17: "done\n" (5 bytes)
  100 c, 111 c, 110 c, 101 c, 10 c,
  \ 22: mov eax, 1
  $b8 c, 1 d,
  \ 27: mov edi, 1
  $bf c, 1 d,
  \ 32: mov rsi, 0x400078+17 = 0x400089
  $48 c, $be c, $400089 q,
  \ 42: mov edx, 5
  $ba c, 5 d,
  \ 47: syscall
  $0f c, $05 c,
  \ 49: exit
  $b8 c, 60 d,
  $31 c, $ff c,
  $0f c, $05 c, ;

: write-out
  s" loop10b" w/o create-file throw >r
  elf-buf 120 r@ write-file throw
  code-buf code-pos @ r@ write-file throw
  r> close-file throw ;

gen-code
elf-header
write-out
s" chmod +x loop10b" system drop
bye
