\ collatz.fs - Collatz sequence length for 837799
\ Result: 524 steps
\ Tests: conditionals, 64-bit arithmetic, division

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

\ rax=n, rbx=steps
\ if n==1: done
\ if n&1: n=3n+1 else n=n/2
\ steps++
: gen-code
  0 code-pos !
  \ 0: mov eax, 837799
  $b8 c, 837799 d,
  \ 5: xor ebx, ebx (steps=0)
  $31 c, $db c,
  \ loop @ 7:
  \ 7: cmp rax, 1
  $48 c, $83 c, $f8 c, 1 c,
  \ 11: je done (+33)
  $74 c, 33 c,
  \ 13: test al, 1 (check odd)
  $a8 c, 1 c,
  \ 15: jz even (+13)
  $74 c, 13 c,
  \ odd: n = 3n + 1
  \ 17: mov rcx, rax
  $48 c, $89 c, $c1 c,
  \ 20: add rax, rax (2n)
  $48 c, $01 c, $c0 c,
  \ 23: add rax, rcx (3n)
  $48 c, $01 c, $c8 c,
  \ 26: inc rax (3n+1)
  $48 c, $ff c, $c0 c,
  \ 29: jmp next (+6)
  $eb c, 6 c,
  \ even @ 31: shr rax, 1
  $48 c, $d1 c, $e8 c,
  \ 34: nop nop nop (padding for jump alignment)
  $90 c, $90 c, $90 c,
  \ next @ 37: inc ebx
  $ff c, $c3 c,
  \ 39: jmp loop (-34 = 0xde)
  $eb c, $de c,
  \ done @ 41: (but we calculated +33 from 13, so 13+2+33=48... recalc)
  \ Actually let me recalculate offsets
  ;

\ Let me redo with careful offset tracking
: gen-code
  0 code-pos !
  \ 0-4: mov eax, 837799 (5 bytes)
  $b8 c, 837799 d,
  \ 5-6: xor ebx, ebx (2 bytes)
  $31 c, $db c,
  \ loop @ 7
  \ 7-10: cmp rax, 1 (4 bytes)
  $48 c, $83 c, $f8 c, 1 c,
  \ 11-12: je done (2 bytes) -> target 46, offset=46-13=33
  $74 c, 33 c,
  \ 13-14: test al, 1 (2 bytes)
  $a8 c, 1 c,
  \ 15-16: jz even (2 bytes) -> target 30, offset=30-17=13
  $74 c, 13 c,
  \ odd: 17-19: mov rcx, rax (3 bytes)
  $48 c, $89 c, $c1 c,
  \ 20-22: add rax, rax (3 bytes)
  $48 c, $01 c, $c0 c,
  \ 23-25: add rax, rcx (3 bytes)
  $48 c, $01 c, $c8 c,
  \ 26-28: inc rax (3 bytes)
  $48 c, $ff c, $c0 c,
  \ 29: jmp next (2 bytes) -> target 35, offset=35-31=4
  $eb c, 4 c,
  \ even @ 31: shr rax, 1 (3 bytes)
  $48 c, $d1 c, $e8 c,
  \ 34: nop (1 byte padding)
  $90 c,
  \ next @ 35: inc ebx (2 bytes)
  $ff c, $c3 c,
  \ 37-38: jmp loop (2 bytes) -> target 7, offset=7-39=-32=0xe0
  $eb c, $e0 c,
  \ done @ 39 (need to fix je offset: from 13, target 39, offset=39-13=26)
  \ Hmm, I had 33 but it should be 26. Let me fix.
  ;

\ Third attempt with correct offsets
: gen-code
  0 code-pos !
  \ 0: mov eax, 837799
  $b8 c, 837799 d,
  \ 5: xor ebx, ebx
  $31 c, $db c,
  \ 7: cmp rax, 1
  $48 c, $83 c, $f8 c, 1 c,
  \ 11: je done -> 39, from 13, offset=26
  $74 c, 26 c,
  \ 13: test al, 1
  $a8 c, 1 c,
  \ 15: jz even -> 31, from 17, offset=14
  $74 c, 14 c,
  \ 17: mov rcx, rax
  $48 c, $89 c, $c1 c,
  \ 20: add rax, rax
  $48 c, $01 c, $c0 c,
  \ 23: add rax, rcx
  $48 c, $01 c, $c8 c,
  \ 26: inc rax
  $48 c, $ff c, $c0 c,
  \ 29: jmp next -> 35, from 31, offset=4
  $eb c, 4 c,
  \ 31: shr rax, 1
  $48 c, $d1 c, $e8 c,
  \ 34: nop
  $90 c,
  \ 35: inc ebx
  $ff c, $c3 c,
  \ 37: jmp loop -> 7, from 39, offset=-32=0xe0
  $eb c, $e0 c,
  \ 39: mov eax, ebx
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
  s" collatz" w/o create-file throw >r
  elf-buf 120 r@ write-file throw
  code-buf code-pos @ r@ write-file throw
  r> close-file throw ;

gen-code
elf-header
write-out
s" chmod +x collatz" system drop
bye
