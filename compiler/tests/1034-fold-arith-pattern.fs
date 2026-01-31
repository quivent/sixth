\ Test 1034: Constant folding + fusion - arith benchmark pattern
\ REGRESSION: The exact pattern from the arith benchmark inner loop:
\ runtime_val 3 * 7 + $FFFFFF and
\ This exercises fusion (3 * → imul imm, 7 + → add imm, $FFFFFF and → and imm)
\ The benchmark went from 0.22s to 0.10s with this optimization.
: step ( n -- n ) 3 * 7 + $FFFFFF and ;
: main 1 step step step step step
  dup 1090 = 0= if begin again then drop ;
