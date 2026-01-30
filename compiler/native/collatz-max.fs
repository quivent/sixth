\ collatz-max.fs - Find starting number with longest Collatz sequence under 1M
\ Result: 837799 (with 524 steps)
\ Tests: nested loops, tracking maximum, complex control flow

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

\ r12=best_n, r13=best_steps, r14=current_n (outer loop)
\ rax=n (inner), rbx=steps
: gen-code
  0 code-pos !
  \ 0: xor r12d, r12d (best_n=0)
  $45 c, $31 c, $e4 c,
  \ 3: xor r13d, r13d (best_steps=0)
  $45 c, $31 c, $ed c,
  \ 6: mov r14d, 1 (current_n=1)
  $41 c, $be c, 1 d,

  \ outer @ 12:
  \ 12: mov rax, r14 (n = current_n)
  $4c c, $89 c, $f0 c,
  \ 15: xor ebx, ebx (steps=0)
  $31 c, $db c,

  \ inner @ 17:
  \ 17: cmp rax, 1
  $48 c, $83 c, $f8 c, 1 c,
  \ 21: je inner_done (+28 -> 51)
  $74 c, 28 c,
  \ 23: test al, 1
  $a8 c, 1 c,
  \ 25: jz even (+14 -> 41)
  $74 c, 14 c,
  \ odd: 27: mov rcx, rax
  $48 c, $89 c, $c1 c,
  \ 30: add rax, rax
  $48 c, $01 c, $c0 c,
  \ 33: add rax, rcx
  $48 c, $01 c, $c8 c,
  \ 36: inc rax
  $48 c, $ff c, $c0 c,
  \ 39: jmp next (+4 -> 45)
  $eb c, 4 c,
  \ even @ 41: shr rax, 1
  $48 c, $d1 c, $e8 c,
  \ 44: nop
  $90 c,
  \ next @ 45: inc ebx
  $ff c, $c3 c,
  \ 47: jmp inner (-32 -> 17 = 0xe0)
  $eb c, $e0 c,

  \ inner_done @ 49 (I miscounted, let me recount):
  \ Actually from 23 (+28) = 51. Let me trace again:
  \ 0-2: 3 bytes, 3-5: 3 bytes, 6-11: 6 bytes = 12
  \ 12-14: 3, 15-16: 2 = 17
  \ 17-20: 4, 21-22: 2, 23-24: 2, 25-26: 2 = 27
  \ 27-29: 3, 30-32: 3, 33-35: 3, 36-38: 3, 39-40: 2 = 41
  \ 41-43: 3, 44: 1, 45-46: 2, 47-48: 2 = 49
  \ So inner_done should be at 49, but je at 21 goes +28 from 23 = 51
  \ Need to fix offset: target=49, from=23, offset=26
  ;

: gen-code
  0 code-pos !
  \ 0: xor r12d, r12d
  $45 c, $31 c, $e4 c,
  \ 3: xor r13d, r13d
  $45 c, $31 c, $ed c,
  \ 6: mov r14d, 1
  $41 c, $be c, 1 d,
  \ outer @ 12:
  \ 12: mov rax, r14
  $4c c, $89 c, $f0 c,
  \ 15: xor ebx, ebx
  $31 c, $db c,
  \ inner @ 17:
  \ 17: cmp rax, 1
  $48 c, $83 c, $f8 c, 1 c,
  \ 21: je inner_done -> 49, from 23, offset=26
  $74 c, 26 c,
  \ 23: test al, 1
  $a8 c, 1 c,
  \ 25: jz even -> 41, from 27, offset=14
  $74 c, 14 c,
  \ 27: mov rcx, rax
  $48 c, $89 c, $c1 c,
  \ 30: add rax, rax
  $48 c, $01 c, $c0 c,
  \ 33: add rax, rcx
  $48 c, $01 c, $c8 c,
  \ 36: inc rax
  $48 c, $ff c, $c0 c,
  \ 39: jmp next -> 45, from 41, offset=4
  $eb c, 4 c,
  \ 41: shr rax, 1
  $48 c, $d1 c, $e8 c,
  \ 44: nop
  $90 c,
  \ 45: inc ebx
  $ff c, $c3 c,
  \ 47: jmp inner -> 17, from 49, offset=-32=0xe0
  $eb c, $e0 c,

  \ inner_done @ 49:
  \ 49: cmp ebx, r13d (steps > best_steps?)
  $44 c, $39 c, $eb c,
  \ 52: jle skip_update -> 61, from 54, offset=7
  $7e c, 7 c,
  \ 54: mov r13d, ebx
  $41 c, $89 c, $dd c,
  \ 57: mov r12, r14
  $4d c, $89 c, $f4 c,
  \ 60: nop
  $90 c,

  \ skip_update @ 61:
  \ 61: inc r14d
  $41 c, $ff c, $c6 c,
  \ 64: cmp r14d, 1000000
  $41 c, $81 c, $fe c, 1000000 d,
  \ 71: jl outer -> 12, from 73, offset=-61=0xc3
  $7c c, $c3 c,

  \ done @ 73: mov eax, r12d
  $44 c, $89 c, $e0 c,
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
  s" collatz-max" w/o create-file throw >r
  elf-buf 120 r@ write-file throw
  code-buf code-pos @ r@ write-file throw
  r> close-file throw ;

gen-code
elf-header
write-out
s" chmod +x collatz-max" system drop
bye
