\ tf.fs - Fifth Native Compiler Library
\ Emit x86_64 Linux ELF binaries with zero dependencies

\ === Buffers ===
create code-buf 4096 allot
variable code-pos  0 code-pos !
create elf-buf 256 allot
variable elf-pos  0 elf-pos !

\ === Code emission ===
: c, ( b -- ) code-buf code-pos @ + c!  1 code-pos +! ;
: d, ( d -- ) dup c, 8 rshift dup c, 8 rshift dup c, 8 rshift c, ;

\ === ELF emission ===
: e, ( b -- ) elf-buf elf-pos @ + c!  1 elf-pos +! ;
: e2, ( w -- ) dup e, 8 rshift e, ;
: e4, ( d -- ) dup e2, 16 rshift e2, ;
: e8, ( q -- ) dup e4, 32 rshift e4, ;

\ === ELF header (64 bytes) + Program header (56 bytes) = 120 bytes ===
: elf-header ( -- )
  0 elf-pos !
  $7f e, 69 e, 76 e, 70 e,         \ magic: 0x7F "ELF"
  2 e, 1 e, 1 e, 0 e,               \ class=64, little-endian, v1, SYSV
  0 e8,                             \ padding
  2 e2, $3e e2, 1 e4,               \ type=exec, machine=x86_64, version
  $400078 e8,                       \ entry point (0x400000 + 120)
  64 e8, 0 e8, 0 e4,                \ phoff=64, shoff=0, flags=0
  64 e2, 56 e2, 1 e2,               \ ehsize=64, phentsize=56, phnum=1
  0 e2, 0 e2, 0 e2,                 \ shentsize, shnum, shstrndx (none)
  \ Program header
  1 e4, 5 e4, 0 e8,                 \ type=LOAD, flags=RX, offset=0
  $400000 e8, $400000 e8,           \ vaddr, paddr
  120 code-pos @ + dup e8, e8,      \ filesz, memsz
  $1000 e8, ;                       \ align

\ === Write ELF binary ===
: write-elf ( addr u -- )
  w/o create-file throw >r
  elf-buf 120 r@ write-file throw
  code-buf code-pos @ r@ write-file throw
  r> close-file throw ;

\ === Common code patterns ===

\ Print number in rax, uses r8 as digit counter
: emit-print-rax ( -- )
  $b9 c, 10 d,              \ mov ecx, 10
  $45 c, $31 c, $c0 c,      \ xor r8d, r8d
  \ digit_loop:
  $31 c, $d2 c,             \ xor edx, edx
  $f7 c, $f1 c,             \ div ecx
  $83 c, $c2 c, $30 c,      \ add edx, '0'
  $52 c,                    \ push rdx
  $41 c, $ff c, $c0 c,      \ inc r8d
  $85 c, $c0 c,             \ test eax, eax
  $75 c, $f1 c,             \ jnz digit_loop
  \ print_loop:
  $b8 c, 1 d,               \ mov eax, 1 (write)
  $bf c, 1 d,               \ mov edi, 1 (stdout)
  $48 c, $89 c, $e6 c,      \ mov rsi, rsp
  $ba c, 1 d,               \ mov edx, 1
  $0f c, $05 c,             \ syscall
  $58 c,                    \ pop
  $41 c, $ff c, $c8 c,      \ dec r8d
  $75 c, $e6 c,             \ jnz print_loop
  ;

\ Print newline
: emit-newline ( -- )
  $6a c, 10 c,              \ push 10
  $b8 c, 1 d,               \ mov eax, 1
  $bf c, 1 d,               \ mov edi, 1
  $48 c, $89 c, $e6 c,      \ mov rsi, rsp
  $ba c, 1 d,               \ mov edx, 1
  $0f c, $05 c,             \ syscall
  $58 c, ;                  \ pop

\ Exit with code 0
: emit-exit ( -- )
  $b8 c, 60 d,              \ mov eax, 60
  $31 c, $ff c,             \ xor edi, edi
  $0f c, $05 c, ;           \ syscall

\ === Example usage ===
\ : my-program
\   0 code-pos !
\   ... emit code ...
\   emit-print-rax
\   emit-newline
\   emit-exit ;
\
\ my-program
\ elf-header
\ s" my-binary" write-elf
\ s" chmod +x my-binary" system drop
