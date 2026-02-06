\ expect: 0
\ Test: Complex forward references with recursion
\ Forward ref + mutual recursion + variables combined

variable fib-n
variable fib-result

: fib-helper ( n -- fib[n] )
  dup 2 < if exit then
  dup 1 - fib-memo
  swap 2 - fib-memo + ;

: fib-memo ( n -- fib[n] )
  dup fib-n @ = if drop fib-result @ exit then
  dup fib-n !
  fib-helper
  dup fib-result ! ;

: check ( -- n )
  0 fib-memo 0 <> if 1 exit then
  1 fib-memo 1 <> if 2 exit then
  2 fib-memo 1 <> if 3 exit then
  5 fib-memo 5 <> if 4 exit then
  10 fib-memo 55 <> if 5 exit then
  0 ;

: main check ;
