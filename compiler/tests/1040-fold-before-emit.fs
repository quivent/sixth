\ expect: A
\ Test 1040: ct-flush before I/O (emit)
\ REGRESSION: I/O words must flush the ct-stack. A folded constant
\ passed to emit must produce correct output.
: main 65 emit cr ;
