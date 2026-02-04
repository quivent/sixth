\ bench.fs - Sixth vs GCC benchmark runner
\ Compile: ./engine/fifth compiler/sixth.fs compiler/bench/bench.fs ./bench
\ Run: ./bench

\ Buffer for command building
create cmd-buf 512 allot
variable cmd-len

: cmd-reset 0 cmd-len ! ;
: cmd+ ( addr u -- ) cmd-buf cmd-len @ + swap dup cmd-len +! move ;
: cmd$ ( -- addr u ) cmd-buf cmd-len @ ;

\ Timing via clock_gettime (syscall 228)
create ts1 16 allot
create ts2 16 allot

: clock-ns ( ts-addr -- )
  >r
  228       \ syscall number (clock_gettime)
  0         \ CLOCK_MONOTONIC = 0? Actually CLOCK_REALTIME=0, MONOTONIC=1
  r>        \ timespec pointer
  syscall2 drop ;

: ts>ms ( ts-addr -- ms )
  dup @ 1000 * swap 8 + @ 1000000 / + ;

: elapsed-ms ( -- ms )
  ts2 ts>ms ts1 ts>ms - ;

\ Run command, return exit code
: run ( addr u -- rc )
  cmd-reset cmd+ cmd$ system-rc ;

\ File existence check
: file-exists? ( addr u -- flag )
  cmd-reset s" test -f " cmd+ cmd+ cmd$ system-rc 0= ;

\ Results storage
create results 256 40 * allot  \ 256 entries, 40 bytes each
variable result-count  0 result-count !

\ Result entry: name[20] + status[4] + cs[4] + cg[4] + rs[4] + rg[4] = 40 bytes
: result-entry ( i -- addr ) 40 * results + ;
: result-name ( i -- addr ) result-entry ;
: result-status ( i -- addr ) result-entry 20 + ;
: result-cs ( i -- addr ) result-entry 24 + ;
: result-cg ( i -- addr ) result-entry 28 + ;
: result-rs ( i -- addr ) result-entry 32 + ;
: result-rg ( i -- addr ) result-entry 36 + ;

\ Store result
: add-result ( name-addr name-u status cs cg rs rg -- )
  result-count @ 256 >= if 2drop 2drop 2drop drop exit then
  result-count @ >r
  r@ result-rg !
  r@ result-rs !
  r@ result-cg !
  r@ result-cs !
  r@ result-status !
  r@ result-name 20 0 fill
  dup 19 > if drop 19 then
  r> result-name swap move
  1 result-count +! ;

\ Benchmark names (static)
create bench-name 64 allot
variable bench-name-len

: set-bench-name ( addr u -- )
  dup 63 > if drop 63 then
  dup bench-name-len !
  bench-name swap move ;

: bench-name$ ( -- addr u ) bench-name bench-name-len @ ;

\ Run one benchmark
: run-bench ( name-addr name-u -- )
  set-bench-name

  \ Check if .c file exists
  cmd-reset s" compiler/bench/" cmd+ bench-name$ cmd+ s" .c" cmd+
  cmd$ file-exists? 0= if exit then

  \ Compile with Sixth
  ts1 clock-ns
  cmd-reset
  s" ./engine/fifth compiler/sixth.fs compiler/bench/" cmd+
  bench-name$ cmd+ s" .fs /tmp/b_sixth >/dev/null 2>&1" cmd+
  cmd$ system-rc
  ts2 clock-ns
  0<> if
    bench-name$ 1 0 0 0 0 add-result  \ CFAIL
    exit
  then
  elapsed-ms >r  \ cs on return stack

  \ Compile with GCC
  ts1 clock-ns
  cmd-reset
  s" gcc -O2 -o /tmp/b_gcc compiler/bench/" cmd+
  bench-name$ cmd+ s" .c 2>/dev/null" cmd+
  cmd$ system-rc
  ts2 clock-ns
  0<> if
    bench-name$ 2 r> 0 0 0 add-result  \ GCFAIL
    exit
  then
  elapsed-ms >r  \ cg on return stack, cs below

  \ Run Sixth binary
  ts1 clock-ns
  s" timeout 30 /tmp/b_sixth </dev/null >/dev/null 2>&1" system-rc
  ts2 clock-ns
  0<> if
    bench-name$ 3 r> r> swap 0 0 add-result  \ RFAIL
    exit
  then
  elapsed-ms >r  \ rs on return stack

  \ Run GCC binary
  ts1 clock-ns
  s" timeout 30 /tmp/b_gcc </dev/null >/dev/null 2>&1" system-rc
  ts2 clock-ns
  0<> if
    bench-name$ 4 r> r> r> -rot 0 add-result  \ GRFAIL
    exit
  then
  elapsed-ms  \ rg

  \ Success - store all times
  bench-name$ 0 r> r> r> -rot rot add-result ;

\ Print number right-aligned in field
: .r ( n width -- )
  >r dup abs 0 <# #s rot sign #> r> over - spaces type ;

\ Print results
: print-results ( -- )
  cr
  ." ══════════════════════════════════════════════════════════════" cr
  ."                   SIXTH vs GCC -O2 BENCHMARK" cr
  ." ══════════════════════════════════════════════════════════════" cr
  cr
  ."                      Sixth    GCC-O2    Ratio" cr
  ." ──────────────────────────────────────────────────────────────" cr

  \ Aggregate stats
  0 0 0 0  \ sum-cs sum-cg sum-rs sum-rg
  0 0 0    \ passed cfail rfail

  result-count @ 0 ?do
    i result-status @ case
      0 of  \ PASS
        rot 1+ -rot  \ increment passed
        i result-cs @ 4 pick + >r drop r>
        i result-cg @ 3 pick + >r nip r>
        i result-rs @ 2 pick + >r rot drop r> -rot
        i result-rg @ over + >r nip r>
      endof
      1 of rot drop 1+ -rot endof  \ CFAIL
      3 of 1+ endof  \ RFAIL
    endcase
  loop

  \ sum-cs sum-cg sum-rs sum-rg passed cfail rfail
  ." Compile (total)   "
  5 pick 6 .r ." ms  " 4 pick 6 .r ." ms" cr
  ." Runtime (total)   "
  3 pick 6 .r ." ms  " 2 pick 6 .r ." ms" cr
  ." ──────────────────────────────────────────────────────────────" cr
  cr
  ." Passed: " dup . ." / " result-count @ . cr
  ." Compile failures: " over . cr
  ." Runtime failures: " . cr
  drop drop drop drop drop drop ;

\ Benchmark list (hardcoded subset for now)
: run-all
  s" ack" run-bench
  s" arith" run-bench
  s" bellman" run-bench
  s" bottles" run-bench
  s" call" run-bench
  s" catalan" run-bench
  s" collatz" run-bench
  s" countdown" run-bench
  s" countup" run-bench
  s" crosssum" run-bench
  s" dfs" run-bench
  s" dijkstra" run-bench
  s" doloop1" run-bench
  s" factorial" run-bench
  s" fib40" run-bench
  s" fibonacci" run-bench
  s" fizzbuzz" run-bench
  s" gcd" run-bench
  s" gcdloop" run-bench
  s" prime" run-bench
  s" sum" run-bench
  s" sieve" run-bench
  ;

: main
  ." Running benchmarks..." cr
  run-all
  print-results ;
