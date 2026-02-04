\ Sixth vs GCC -O2 Benchmark Suite (Simple Version)
\ Run: ./engine/fifth compiler/bench/benchmark.fs

variable t0
variable t1
: ms@ clock-ms ;
: elapsed t1 @ t0 @ - ;

\ Run and time a single benchmark
: bench ( -- )
  ." arith:    "
  ms@ t0 !
  s" ./engine/fifth compiler/sixth.fs compiler/bench/arith.fs /tmp/b 2>/dev/null" system drop
  ms@ t1 !
  ." 6thC=" elapsed . ." ms "
  ms@ t0 !
  s" gcc -O2 -o /tmp/c compiler/bench/arith.c 2>/dev/null" system drop
  ms@ t1 !
  ." gccC=" elapsed . ." ms "
  ms@ t0 !
  s" /tmp/b >/dev/null 2>&1" system drop
  ms@ t1 !
  ." 6thR=" elapsed . ." ms "
  ms@ t0 !
  s" /tmp/c >/dev/null 2>&1" system drop
  ms@ t1 !
  ." gccR=" elapsed . ." ms" cr

  ." fib40:    "
  ms@ t0 !
  s" ./engine/fifth compiler/sixth.fs compiler/bench/fib40.fs /tmp/b 2>/dev/null" system drop
  ms@ t1 !
  ." 6thC=" elapsed . ." ms "
  ms@ t0 !
  s" gcc -O2 -o /tmp/c compiler/bench/fib40.c 2>/dev/null" system drop
  ms@ t1 !
  ." gccC=" elapsed . ." ms "
  ms@ t0 !
  s" /tmp/b >/dev/null 2>&1" system drop
  ms@ t1 !
  ." 6thR=" elapsed . ." ms "
  ms@ t0 !
  s" /tmp/c >/dev/null 2>&1" system drop
  ms@ t1 !
  ." gccR=" elapsed . ." ms" cr

  ." collatz:  "
  ms@ t0 !
  s" ./engine/fifth compiler/sixth.fs compiler/bench/collatz.fs /tmp/b 2>/dev/null" system drop
  ms@ t1 !
  ." 6thC=" elapsed . ." ms "
  ms@ t0 !
  s" gcc -O2 -o /tmp/c compiler/bench/collatz.c 2>/dev/null" system drop
  ms@ t1 !
  ." gccC=" elapsed . ." ms "
  ms@ t0 !
  s" /tmp/b >/dev/null 2>&1" system drop
  ms@ t1 !
  ." 6thR=" elapsed . ." ms "
  ms@ t0 !
  s" /tmp/c >/dev/null 2>&1" system drop
  ms@ t1 !
  ." gccR=" elapsed . ." ms" cr

  ." call:     "
  ms@ t0 !
  s" ./engine/fifth compiler/sixth.fs compiler/bench/call.fs /tmp/b 2>/dev/null" system drop
  ms@ t1 !
  ." 6thC=" elapsed . ." ms "
  ms@ t0 !
  s" gcc -O2 -o /tmp/c compiler/bench/call.c 2>/dev/null" system drop
  ms@ t1 !
  ." gccC=" elapsed . ." ms "
  ms@ t0 !
  s" /tmp/b >/dev/null 2>&1" system drop
  ms@ t1 !
  ." 6thR=" elapsed . ." ms "
  ms@ t0 !
  s" /tmp/c >/dev/null 2>&1" system drop
  ms@ t1 !
  ." gccR=" elapsed . ." ms" cr

  ." primes:   "
  ms@ t0 !
  s" ./engine/fifth compiler/sixth.fs compiler/bench/primes.fs /tmp/b 2>/dev/null" system drop
  ms@ t1 !
  ." 6thC=" elapsed . ." ms "
  ms@ t0 !
  s" gcc -O2 -o /tmp/c compiler/bench/primes.c 2>/dev/null" system drop
  ms@ t1 !
  ." gccC=" elapsed . ." ms "
  ms@ t0 !
  s" /tmp/b >/dev/null 2>&1" system drop
  ms@ t1 !
  ." 6thR=" elapsed . ." ms "
  ms@ t0 !
  s" /tmp/c >/dev/null 2>&1" system drop
  ms@ t1 !
  ." gccR=" elapsed . ." ms" cr

  ." ack:      "
  ms@ t0 !
  s" ./engine/fifth compiler/sixth.fs compiler/bench/ack.fs /tmp/b 2>/dev/null" system drop
  ms@ t1 !
  ." 6thC=" elapsed . ." ms "
  ms@ t0 !
  s" gcc -O2 -o /tmp/c compiler/bench/ack.c 2>/dev/null" system drop
  ms@ t1 !
  ." gccC=" elapsed . ." ms "
  ms@ t0 !
  s" /tmp/b >/dev/null 2>&1" system drop
  ms@ t1 !
  ." 6thR=" elapsed . ." ms "
  ms@ t0 !
  s" /tmp/c >/dev/null 2>&1" system drop
  ms@ t1 !
  ." gccR=" elapsed . ." ms" cr

  ." ctrl:     "
  ms@ t0 !
  s" ./engine/fifth compiler/sixth.fs compiler/bench/ctrl.fs /tmp/b 2>/dev/null" system drop
  ms@ t1 !
  ." 6thC=" elapsed . ." ms "
  ms@ t0 !
  s" gcc -O2 -o /tmp/c compiler/bench/ctrl.c 2>/dev/null" system drop
  ms@ t1 !
  ." gccC=" elapsed . ." ms "
  ms@ t0 !
  s" /tmp/b >/dev/null 2>&1" system drop
  ms@ t1 !
  ." 6thR=" elapsed . ." ms "
  ms@ t0 !
  s" /tmp/c >/dev/null 2>&1" system drop
  ms@ t1 !
  ." gccR=" elapsed . ." ms" cr

  ." tak:      "
  ms@ t0 !
  s" ./engine/fifth compiler/sixth.fs compiler/bench/tak.fs /tmp/b 2>/dev/null" system drop
  ms@ t1 !
  ." 6thC=" elapsed . ." ms "
  ms@ t0 !
  s" gcc -O2 -o /tmp/c compiler/bench/tak.c 2>/dev/null" system drop
  ms@ t1 !
  ." gccC=" elapsed . ." ms "
  ms@ t0 !
  s" /tmp/b >/dev/null 2>&1" system drop
  ms@ t1 !
  ." 6thR=" elapsed . ." ms "
  ms@ t0 !
  s" /tmp/c >/dev/null 2>&1" system drop
  ms@ t1 !
  ." gccR=" elapsed . ." ms" cr
;

." SIXTH vs GCC -O2 BENCHMARKS" cr
." ===========================" cr
bench
