\ expect: 89
\ Complex: Fibonacci with tail accumulator
\ fib-iter exercises stack + recursive calls
: fib-done drop drop ;
: fib-iter
  dup 0= if fib-done exit then
  1- >r over + swap r> fib-iter ;
: fib
  dup 1 < if exit then
  0 1 rot fib-iter ;
: main 11 fib ;
