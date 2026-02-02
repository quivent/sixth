\ fib-std.fs - Iterative fibonacci WITHOUT tuck+
\ Same computation as fib.fs but standard Forth only.
\ C equivalent: t=a+b; a=b; b=t; — gcc uses registers, no memory.
\ tuck+ is xadd (1 insn). swap over + is 5+ insns minimum.
\ Run fib(45) for measurable time.
: fib ( n -- f )
  0 1 rot 0 do
    swap over +
  loop drop ;
: main ( -- ) 1000000000 fib . cr ;
