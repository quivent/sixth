\ stress-words-09.fs - Tail call patterns (iterative fib)
\ expect: 55
\ Fibonacci: fib(10) = 55
variable fa variable fb variable fn
: fib-step fa @ fb @ + fb @ fa ! fb ! ;
: fib-loop fn @ 0> if fib-step fn @ 1- fn ! fib-loop then ;
: fib 1- fn ! 0 fa ! 1 fb ! fib-loop fb @ ;
: main 10 fib ;
