\ Test 1024: ct-flush before dup
\ REGRESSION: dup must flush ct-stack first so the literal is in rax
\ before being duplicated.
: main 21 dup + 42 = 0= if begin again then ;
