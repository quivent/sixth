\ expect: 55
\ Fibonacci(10) using variables, no recursion
variable fa variable fb variable fc
: fib ( n -- fib )
  0 fa ! 1 fb !
  0 do
    fa @ fb @ + fc !
    fb @ fa !
    fc @ fb !
  loop
  fa @ ;
: main 10 fib . cr ;
