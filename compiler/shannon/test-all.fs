\ Test all Shannon modules load together

create code-buf 256 allot
variable code-pos  0 code-pos !
: c, code-buf code-pos @ + c!  1 code-pos +! ;
: d, dup c, 8 rshift dup c, 8 rshift dup c, 8 rshift c, ;
: q, dup d, 32 rshift d, ;

\ String comparison (needed by compile.fs)
: str= ( addr1 u1 addr2 u2 -- flag )
  rot over <> if 2drop drop false exit then
  dup 0= if 2drop drop true exit then
  0 ?do
    over i + c@ over i + c@ <> if 2drop false unloop exit then
  loop
  2drop true ;

." Loading Shannon modules..." cr
." - asm.fs" cr
include compiler/shannon/asm.fs
." - stack.fs" cr
include compiler/shannon/stack.fs
." - prims.fs" cr
include compiler/shannon/prims.fs
." - opt-fold.fs" cr
include compiler/shannon/opt-fold.fs
." - opt-fuse.fs" cr
include compiler/shannon/opt-fuse.fs
." - opt-swap.fs" cr
include compiler/shannon/opt-swap.fs
." - dispatch.fs" cr
include compiler/shannon/dispatch.fs
." - compile.fs" cr
include compiler/shannon/compile.fs

." All modules loaded!" cr
." Builtins: " count-builtins . cr
bye
