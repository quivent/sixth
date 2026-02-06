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

: patch-call ( target from -- )
  \ Patch a BL instruction at 'from' to call 'target'
  \ BL uses same offset encoding as B (26-bit signed, bits 25:0)
  patch-branch-uncond ;

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

: emit-exit ( -- )
  \ Early return from word - same as gen-ret
  gen-ret ;

\ ============================================================
\ DO / LOOP CONTROL FLOW
\ ============================================================
\ Return stack layout: ... limit index (index on top at [X28])
\ Standard Forth: limit index do ... loop
\ Loops while index < limit

: gen-do ( -- orig dest )
  \ ( limit index -- ) ( R: -- limit index )
  \ Push limit first (NOS), then index (TOS), so index ends up on top
  pop-nos                            \ X9 = limit (was NOS)
  9 28 -8 arm-str-pre emit32         \ STR X9, [X28, #-8]! (push limit)
  19 28 -8 arm-str-pre emit32        \ STR X19, [X28, #-8]! (push index)
  emit-drop                          \ pop index from data stack
  \ Return stack now: ... limit index  with [X28]=index, [X28,#8]=limit
  \ Zero-iter check: if index >= limit, skip (works for ascending LOOP)
  \ gen-+loop will NOP this out since it handles both directions
  9 28 0 arm-ldr-off emit32          \ LDR X9, [X28] (index)
  10 28 1 arm-ldr-off emit32         \ LDR X10, [X28, #8] (limit)
  9 10 arm-cmp-reg emit32            \ CMP X9, X10 (index - limit)
  code-here                          \ orig = position of B.GE for patching
  $5400000A emit32                   \ B.GE placeholder (cond=GE=0xA)
  code-here ;                        \ dest = loop body start

: gen-loop ( orig dest -- )
  \ Increment index, compare with limit, branch back if index < limit
  \ Return stack: [X28]=index, [X28+8]=limit (offset in cells, not bytes)
  swap cf-push                       \ save orig to cf-stack for patching later
  9 28 0 arm-ldr-off emit32          \ LDR X9, [X28] (index)
  9 9 1 arm-add-imm emit32           \ ADD X9, X9, #1
  9 28 0 arm-str-off emit32          \ STR X9, [X28] (store back)
  10 28 1 arm-ldr-off emit32         \ LDR X10, [X28, #8] (limit at offset 1*8)
  \ Compare: if index < limit, continue looping
  9 10 arm-cmp-reg emit32            \ CMP X9, X10 (index - limit)
  \ B.LT back to dest (dest is on data stack)
  code-here -                        \ offset = dest - code-here (bytes, negative)
  4 /                                \ offset in instructions
  $7FFFF and 5 lshift                \ mask to 19 bits, shift to position
  $54000000 or                       \ B.cond base opcode
  $B or                              \ cond = LT (0xB)
  emit32
  \ Patch B.GE from gen-do to jump to cleanup
  code-here cf-pop patch-branch
  \ Clean up return stack: drop limit and index
  28 28 16 arm-add-imm emit32 ;      \ ADD X28, X28, #16

: gen-+loop ( orig dest -- )
  \ Add TOS to index, check for bounds crossing, branch back if not done
  \ ( n -- ) adds n to index
  \ Return stack: [X28]=index, [X28+8]=limit (offset in cells)
  \ NOP out the B.GE from gen-do (it's wrong for negative steps)
  swap code-buf + $D503201F swap l!  \ Write NOP at orig position
  9 28 0 arm-ldr-off emit32          \ LDR X9, [X28] (old index)
  10 28 1 arm-ldr-off emit32         \ LDR X10, [X28, #8] (limit)
  \ new_index = old_index + step (TOS)
  11 9 19 arm-add-reg emit32         \ ADD X11, X9, X19 (new index)
  11 28 0 arm-str-off emit32         \ STR X11, [X28] (store new index)
  emit-drop                          \ pop increment from data stack
  \ ANS Forth boundary crossing check:
  \ Exit when index crosses from (limit-1) to limit in either direction
  \ Check: (old_index - limit) XOR (new_index - limit) < 0 means crossed
  9 9 10 arm-sub-reg emit32          \ SUB X9, X9, X10 (old_diff = old - limit)
  11 11 10 arm-sub-reg emit32        \ SUB X11, X11, X10 (new_diff = new - limit)
  9 9 11 arm-eor-reg emit32          \ EOR X9, X9, X11 (xor result)
  9 0 arm-cmp-imm emit32             \ CMP X9, #0 (set flags based on XOR result)
  \ B.PL back to dest (continue if positive = no sign change = no crossing)
  code-here -                        \ offset = dest - code-here (bytes, negative)
  4 /                                \ offset in instructions
  $7FFFF and 5 lshift                \ mask to 19 bits, shift to position
  $54000000 or                       \ B.cond base opcode
  $5 or                              \ cond = PL (positive or zero, 0x5)
  emit32
  \ Clean up return stack
  28 28 16 arm-add-imm emit32 ;

: emit-i ( -- )  \ ( -- index ) copy current loop index to data stack
  push-tos                           \ save TOS
  19 28 0 arm-ldr-off emit32 ;       \ LDR X19, [X28] (index is on top)

: emit-j ( -- )  \ ( -- index ) copy outer loop index to data stack
  push-tos                           \ save TOS
  19 28 2 arm-ldr-off emit32 ;       \ LDR X19, [X28, #16] (skip inner index+limit = 2*8)

: emit-unloop ( -- )  \ ( R: limit index -- ) remove loop params from return stack
  28 28 16 arm-add-imm emit32 ;      \ ADD X28, X28, #16

: gen-leave ( -- )
  \ Set index = limit to exit on next loop iteration
  9 28 1 arm-ldr-off emit32          \ LDR X9, [X28, #8] (limit at offset 1*8)
  9 28 0 arm-str-off emit32 ;        \ STR X9, [X28] (index = limit)
