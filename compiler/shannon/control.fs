\ control.fs - Control Flow Code Generation (Shannon Layer 2b)
\ Ported from sixth.fs for the Shannon Architecture
\
\ Requires: asm.fs, stack.fs, c,, d,, code-here

\ ============================================================
\ CONTROL FLOW STACK
\ ============================================================
\ Used for forward reference patching (if/then/else, loops)

create cf-stack 256 cells allot
variable cf-sp  0 cf-sp !

: cf-push ( n -- ) cf-stack cf-sp @ cells + ! 1 cf-sp +! ;
: cf-pop ( -- n ) -1 cf-sp +! cf-stack cf-sp @ cells + @ ;

\ ============================================================
\ FORWARD REFERENCE PATCHING
\ ============================================================

: patch-rel32 ( target from -- )
  \ Patch a 32-bit relative offset at 'from' to jump to 'target'
  \ from points to the byte AFTER the 4-byte displacement
  \ displacement = target - from (relative jump)
  \ write location = from - 4 (the 4-byte displacement field)
  tuck -                  \ ( from displacement ) where displacement = target - from
  swap 4 - code-buf + d! ; \ write displacement at from-4

\ ============================================================
\ IF / THEN / ELSE
\ ============================================================

: gen-if ( -- orig )
  \ Test TOS, jump if zero
  \ mov rdi, rax; pop-tos; test rdi,rdi; jz rel32
  $48 c, $89 c, $c7 c,    \ mov rdi, rax
  pop-val
  $48 c, $85 c, $ff c,    \ test rdi, rdi
  $0f c, $84 c,           \ jz rel32
  0 d,                    \ placeholder
  code-here ;             \ return address to patch

: gen-else ( orig1 -- orig2 )
  \ Jump over else clause, patch if to here
  $e9 c,                  \ jmp rel32
  0 d,                    \ placeholder
  code-here               \ new orig (end of else jump)
  swap                    \ ( orig2 orig1 )
  code-here swap patch-rel32 ;

: gen-then ( orig -- )
  \ Patch forward reference
  code-here swap patch-rel32 ;

\ ============================================================
\ BEGIN / UNTIL / WHILE / REPEAT / AGAIN
\ ============================================================

: gen-begin ( -- dest )
  code-here ;

: gen-until ( dest -- )
  \ Test TOS, jump back if zero
  $48 c, $89 c, $c7 c,    \ mov rdi, rax
  pop-val
  $48 c, $85 c, $ff c,    \ test rdi, rdi
  $0f c, $84 c,           \ jz rel32
  code-here 4 + - d, ;    \ backward displacement

: gen-again ( dest -- )
  \ Unconditional jump back
  $e9 c,                  \ jmp rel32
  code-here 4 + - d, ;    \ backward displacement

: gen-while ( dest -- orig dest )
  \ Test TOS, conditional forward jump, keep dest for repeat
  gen-if                  \ ( dest orig )
  swap ;                  \ ( orig dest )

: gen-repeat ( orig dest -- )
  \ Jump back to begin, patch while
  gen-again               \ jump to dest
  gen-then ;              \ patch orig

\ ============================================================
\ FUSED COMPARE+BRANCH (optimization)
\ ============================================================

: gen-0=if ( -- orig )
  \ test rax,rax; pop-tos; jnz rel32 (branch if NOT zero)
  $48 c, $85 c, $c0 c,    \ test rax, rax
  pop-val
  $0f c, $85 c,           \ jnz rel32
  0 d,
  code-here ;

: gen-0<if ( -- orig )
  \ test rax,rax; pop-tos; jns rel32 (branch if NOT negative)
  $48 c, $85 c, $c0 c,    \ test rax, rax
  pop-val
  $0f c, $89 c,           \ jns rel32
  0 d,
  code-here ;

: gen-<if ( -- orig )
  \ Compare NOS < TOS, branch if false
  \ Load NOS, cmp, pop both, jge
  nos tos cmp-rr          \ cmp rbx, rax
  pop-val pop-val         \ pop both operands
  $0f c, $8d c,           \ jge rel32 (branch if NOT less)
  0 d,
  code-here ;

: gen->if ( -- orig )
  \ Compare NOS > TOS, branch if false
  nos tos cmp-rr          \ cmp rbx, rax
  pop-val pop-val
  $0f c, $8e c,           \ jle rel32 (branch if NOT greater)
  0 d,
  code-here ;

: gen-=if ( -- orig )
  \ Compare NOS = TOS, branch if false
  nos tos cmp-rr          \ cmp rbx, rax
  pop-val pop-val
  $0f c, $85 c,           \ jne rel32 (branch if NOT equal)
  0 d,
  code-here ;

\ ============================================================
\ DO / LOOP (simplified version)
\ ============================================================
\ Uses return stack for loop control

: gen-do ( -- do-addr leave-addr )
  \ ( limit index -- ) R: ( -- limit index )
  \ Push limit and index to return stack
  \ sub rbp, 16; mov [rbp+8], rbx; mov [rbp], rax; pop both
  $48 c, $83 c, $ed c, 16 c,     \ sub rbp, 16
  $48 c, $89 c, $5d c, 8 c,      \ mov [rbp+8], rbx (limit)
  $48 c, $89 c, $45 c, 0 c,      \ mov [rbp], rax (index)
  pop-val pop-val
  code-here                      \ do-addr (loop target)
  0 ;                            \ leave-addr placeholder

: gen-loop ( do-addr leave-addr -- )
  \ Increment index, compare to limit, loop or exit
  \ inc [rbp]; cmp with [rbp+8]; jl do-addr
  $48 c, $ff c, $45 c, 0 c,      \ inc qword [rbp]
  $48 c, $8b c, $45 c, 0 c,      \ mov rax, [rbp] (index)
  $48 c, $3b c, $45 c, 8 c,      \ cmp rax, [rbp+8] (limit)
  $0f c, $8c c,                  \ jl rel32
  swap code-here 4 + - d,        \ backward to do-addr
  \ Clean up return stack
  $48 c, $83 c, $c5 c, 16 c,     \ add rbp, 16
  drop ;                         \ discard leave-addr

: gen-i ( -- )
  \ Push current loop index
  push-val
  $48 c, $8b c, $45 c, 0 c, ;    \ mov rax, [rbp]

: gen-j ( -- )
  \ Push outer loop index
  push-val
  $48 c, $8b c, $45 c, 16 c, ;   \ mov rax, [rbp+16]

: gen-unloop ( -- )
  \ Discard loop parameters
  $48 c, $83 c, $c5 c, 16 c, ;   \ add rbp, 16

