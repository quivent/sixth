\ expect: 0
\ Division by zero - ARM64 SDIV returns 0 for div by zero
\ This is architecture-defined behavior, not an exception
: main 42 0 / ;
