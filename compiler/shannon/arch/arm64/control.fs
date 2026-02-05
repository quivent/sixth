\ control.fs - Control Flow Code Generation for ARM64
\ Requires: asm.fs, stack.fs

\ ============================================================
\ CONTROL FLOW STACK
\ ============================================================

create cf-stack 256 cells allot
variable cf-sp  0 cf-sp !

: cf-push ( n -- ) cf-stack cf-sp @ cells + ! 1 cf-sp +! ;
: cf-pop ( -- n ) -1 cf-sp +! cf-stack cf-sp @ cells + @ ;

\ ============================================================
\ BRANCH INSTRUCTION HELPERS
\ ============================================================

\ CBZ Xt, label (branch if zero) - 19-bit signed offset in instructions
: arm-cbz ( rt offset19 -- insn )
  $7FFFF and 5 lshift swap or $B4000000 or ;

\ CBNZ Xt, label (branch if not zero)
: arm-cbnz ( rt offset19 -- insn )
  $7FFFF and 5 lshift swap or $B5000000 or ;

\ code-here returns offset from code-buf start
: code-here ( -- addr ) code-pos @ ;

\ ============================================================
\ 32-BIT MEMORY ACCESS (ARM64 instructions are 32-bit)
\ ============================================================

\ l@ - fetch 32-bit little-endian value
: l@ ( addr -- u32 )
  dup c@ swap 1+ dup c@ swap 1+ dup c@ swap 1+ c@
  24 lshift swap 16 lshift or swap 8 lshift or or ;

\ l! - store 32-bit little-endian value
: l! ( u32 addr -- )
  over $FF and over c!
  1+ over 8 rshift $FF and over c!
  1+ over 16 rshift $FF and over c!
  1+ swap 24 rshift $FF and swap c! ;

\ ============================================================
\ FORWARD REFERENCE PATCHING
\ ============================================================

: patch-branch ( target from -- )
  \ Patch a CBZ/CBNZ/B instruction at 'from' to branch to 'target'
  \ Both are code-pos offsets (in bytes)
  \ ARM64 branch offset is (target - from) / 4, in the instruction encoding
  swap over -           \ offset in bytes: target - from
  4 /                   \ offset in instructions
  $7FFFF and 5 lshift   \ encode offset in bits [23:5]
  swap code-buf + dup   \ get instruction address
  l@ $FF00001F and      \ clear old offset bits (32-bit read)
  rot or                \ merge new offset
  swap l! ;             \ write back (32-bit write)

: patch-branch-uncond ( target from -- )
  \ Patch an unconditional B instruction
  swap over -
  4 /
  $3FFFFFF and         \ 26-bit offset
  swap code-buf + dup
  l@ $FC000000 and     \ clear old offset bits (32-bit read)
  rot or
  swap l! ;            \ write back (32-bit write)

\ ============================================================
\ IF / THEN / ELSE
\ ============================================================

: gen-if ( -- orig )
  \ Test TOS, consume it, jump forward if zero
  \ Must drop BEFORE branch so flag is consumed regardless of branch taken
  9 19 arm-mov-reg emit32  \ MOV X9, X19 (save flag to scratch)
  emit-drop                \ Drop flag from stack
  9 0 arm-cbz emit32       \ CBZ X9, placeholder (test scratch)
  code-here 4 + ;          \ Return orig such that orig-8 = CBZ position

: gen-else ( orig1 -- orig2 )
  \ Jump over else clause unconditionally, patch 'if' to here
  0 arm-b emit32         \ B placeholder
  code-here 4 +          \ orig2: add 4 so (orig2-8) = B position
  1 or                   \ set bit 0 = unconditional flag
  swap                   \ ( orig2 orig1 )
  code-here swap 8 - patch-branch ; \ patch if's CBZ at orig1-8 to jump here

: gen-then ( orig -- )
  \ Patch forward reference from if/else
  \ Check bit 0: 0 = conditional (CBZ at orig-8), 1 = unconditional (B at orig-8)
  dup 1 and if
    \ Unconditional branch from gen-else
    1 xor                          \ clear flag bit
    code-here swap 8 - patch-branch-uncond
  else
    \ Conditional branch from gen-if (CBZ at orig-8)
    code-here swap 8 - patch-branch
  then ;

\ ============================================================
\ BEGIN / UNTIL / WHILE / REPEAT / AGAIN
\ ============================================================

: gen-begin ( -- dest )
  code-here ;

: gen-until ( dest -- )
  \ Test TOS, consume it, jump back if zero (continue loop while false)
  \ Must drop BEFORE branch so flag isn't left on stack when looping
  9 19 arm-mov-reg emit32  \ MOV X9, X19 (save flag to scratch)
  emit-drop                \ Drop flag from stack
  9 0 arm-cbz emit32       \ CBZ X9, placeholder (test scratch)
  \ Patch CBZ to branch to dest: patch-branch(target=dest, from=CBZ_pos)
  \ CBZ is 4 bytes before code-here (no drop after CBZ now)
  code-here 4 - patch-branch ;

: gen-again ( dest -- )
  \ Unconditional jump back to begin
  \ Offset = (dest - code-here) / 4 (negative for backward branch)
  code-here -            \ dest - code-here (negative)
  4 /                    \ offset in instructions
  arm-b emit32 ;

: gen-while ( dest -- orig dest )
  \ Test TOS, conditional forward jump, keep dest for repeat
  gen-if
  swap ;

: gen-repeat ( orig dest -- )
  \ Jump back to begin, patch while
  gen-again
  gen-then ;

\ ============================================================
\ WORD CALLS (for multi-word definitions)
\ ============================================================

: gen-call ( target -- )
  \ Emit BL to target address (code-pos offset)
  \ offset = (target - code-here) / 4
  code-here -
  4 /
  arm-bl emit32 ;

: gen-word-prologue ( -- )
  \ Save LR to return stack (needed for non-leaf functions)
  \ STR X30, [X28, #-8]!  (pre-indexed, decrement X28 by 8)
  $F81F8F9E emit32 ;

: gen-ret ( -- )
  \ Restore LR from return stack and return
  \ LDR X30, [X28], #8
  $F840879E emit32
  \ RET
  $D65F03C0 emit32 ;
