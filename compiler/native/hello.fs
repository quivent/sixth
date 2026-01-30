\ hello.fs - Hello World
\ Binary size: 172 bytes

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

: gen-code
  0 code-pos !
  \ jmp +14 (skip string)
  $eb c, 14 c,
  \ "Hello, World!\n" = 14 bytes
  72 c, 101 c, 108 c, 108 c, 111 c, 44 c, 32 c,
  87 c, 111 c, 114 c, 108 c, 100 c, 33 c, 10 c,
  \ mov eax, 1 (write)
  $b8 c, 1 d,
  \ mov edi, 1 (stdout)
  $bf c, 1 d,
  \ mov rsi, 0x40007a (string address)
  $48 c, $be c, $40007a q,
  \ mov edx, 14 (length)
  $ba c, 14 d,
  \ syscall
  $0f c, $05 c,
  \ exit 0
  $b8 c, 60 d,
  $31 c, $ff c,
  $0f c, $05 c, ;

: write-out
  s" hello" w/o create-file throw >r
  elf-buf 120 r@ write-file throw
  code-buf code-pos @ r@ write-file throw
  r> close-file throw ;

gen-code
elf-header
write-out
s" chmod +x hello" system drop
bye
