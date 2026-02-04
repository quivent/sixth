\ opt-fuse.fs - Literal fusion optimization (Shannon Layer 3b)
\ Fuses compile-time literals with operations into immediate instructions.
\
\ Requires: asm.fs (RAX, xor-rr-same)
\ Requires: stack.fs (tos, emit-lit)
\ Requires: prims.fs (emit-*-imm words)
\ Requires: opt-fold.fs (ct-pop, ct-depth@)
\
\ When ct-depth = 1 and a binary op is seen, instead of:
\   flush constant -> emit op
\ We do:
\   pop constant -> emit op-immediate
\
\ This saves ~6 bytes per fused op and removes a register move.

\ ============================================================
\ POWER-OF-2 HELPERS
\ ============================================================

: power-of-2? ( n -- flag )
  \ Is n a positive power of 2?
  dup 1 < if drop false exit then
  dup 1- and 0= ;

: log2 ( n -- shift )
  \ Compute floor(log2(n)) for n >= 1
  \ Returns shift amount for power-of-2 multiply -> shift conversion
  0 swap begin dup 1 > while 1 rshift swap 1+ swap repeat drop ;

\ ============================================================
\ FUSED OPERATIONS
\ ============================================================
\ Each fuse-* word pops one constant from ct-stack and emits
\ an immediate instruction operating on TOS (RAX).

: fuse-add ( -- )
  \ ( x -- x+n ) where n is compile-time constant
  ct-pop emit-add-imm ;

: fuse-sub ( -- )
  \ ( x -- x-n ) where n is compile-time constant
  ct-pop emit-sub-imm ;

: fuse-mul ( -- )
  \ ( x -- x*n ) with strength reduction for powers of 2
  ct-pop
  dup 0= if
    drop tos xor-rr-same        \ x * 0 = 0
  else dup 1 = if
    drop                        \ x * 1 = x (emit nothing)
  else dup power-of-2? if
    log2 emit-lshift-imm        \ x * 2^n = x << n
  else
    emit-mul-imm                \ general case: imul rax, rax, n
  then then then ;

: fuse-and ( -- )
  \ ( x -- x&n ) where n is compile-time constant
  ct-pop emit-and-imm ;

: fuse-or ( -- )
  \ ( x -- x|n ) where n is compile-time constant
  ct-pop emit-or-imm ;

: fuse-xor ( -- )
  \ ( x -- x^n ) where n is compile-time constant
  ct-pop
  dup 0= if
    drop                        \ x ^ 0 = x (emit nothing)
  else dup -1 = if
    drop tos not-r              \ x ^ -1 = ~x (invert)
  else
    emit-xor-imm                \ general case
  then then ;

: fuse-lshift ( -- )
  \ ( x -- x<<n ) where n is compile-time constant
  ct-pop emit-lshift-imm ;

: fuse-rshift ( -- )
  \ ( x -- x>>n ) logical right shift where n is compile-time constant
  ct-pop tos shr-ri ;

: fuse-arshift ( -- )
  \ ( x -- x>>>n ) arithmetic right shift where n is compile-time constant
  ct-pop tos sar-ri ;

\ ============================================================
\ FUSE DECISION HELPER
\ ============================================================

: can-fuse? ( -- flag )
  \ Can we fuse the next operation with a literal?
  \ True when exactly one compile-time constant is pending
  ct-depth@ 1 = ;
