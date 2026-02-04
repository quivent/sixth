\ bench/run.fs - Benchmark sixth.fs vs gcc -O2
\ Usage: ./engine/fifth compiler/bench/run.fs
\ Primary: ack (source of truth)

create cmd 256 allot
variable cmd-len

: cmd! 0 cmd-len ! ;
: cmd+ ( a u -- ) cmd cmd-len @ + swap dup cmd-len +! move ;
: cmd$ cmd cmd-len @ ;

: bench ( name$ -- )
  ." === " 2dup type ."  ===" cr
  cmd! s" ./engine/fifth compiler/sixth.fs compiler/bench/" cmd+ 2dup cmd+ s" .fs /tmp/b_s" cmd+ cmd$ system
  cmd! s" gcc -O2 -o /tmp/b_c compiler/bench/" cmd+ 2dup cmd+ s" .c" cmd+ cmd$ system
  ." sixth: " s" /tmp/b_s" system cr
  ." gcc:   " s" /tmp/b_c" system cr
  2drop cr ;

: main
  s" ack"     bench
  s" fib40"   bench
  s" primes"  bench
  s" collatz" bench ;

main bye
