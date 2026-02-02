\ loop-std.fs - Tight countdown WITHOUT 1-nzloop
\ Same computation as loop.fs but standard Forth only.
\ C equivalent: while(n>0) n--; — gcc compiles to dec+jnz.
\ 1-nzloop is dec+jnz (2 insns fused). Standard Forth needs
\ 1- dup test + conditional branch + unconditional branch.
\ This is where the gap should be widest.
: main ( -- )
  1000000000
  begin
    1- dup 0>
  while repeat
  . cr ;
