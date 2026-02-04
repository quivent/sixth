\ opt-swap.fs - Swap elimination optimization (Shannon Layer 3)
\
\ Tracks pending swaps and eliminates them when possible.
\ Key insight: swap followed by commutative op = just the op.
\
\ Requires: prims.fs for emit-swap

\ ============================================================
\ STATE
\ ============================================================

variable swap-pending  0 swap-pending !

\ ============================================================
\ INTERFACE
\ ============================================================

: swap-pending? ( -- flag )
  \ Is there a pending swap?
  swap-pending @ 0 <> ;

: mark-swap ( -- )
  \ Mark that a swap is pending (don't emit yet)
  1 swap-pending ! ;

: clear-swap ( -- )
  \ Clear pending swap (was absorbed by commutative op)
  0 swap-pending ! ;

: flush-swap ( -- )
  \ Emit pending swap if any, then clear
  swap-pending @ if
    emit-swap
    0 swap-pending !
  then ;

: cancel-swap ( -- )
  \ Two swaps cancel: swap swap = nothing
  \ If swap pending, clear it. Otherwise mark one.
  swap-pending @ if
    0 swap-pending !
  else
    1 swap-pending !
  then ;

\ ============================================================
\ ABSORPTION HELPERS
\ ============================================================

\ Commutative operations can absorb a pending swap.
\ Called before emitting +, *, and, or, xor.
: absorb-swap-if-commutative ( -- )
  \ If swap is pending, just clear it - operand order doesn't matter
  0 swap-pending ! ;

\ Non-commutative operations must flush the swap first.
\ Called before emitting /, -, comparisons, memory ops, etc.
: require-swap-flushed ( -- )
  flush-swap ;

