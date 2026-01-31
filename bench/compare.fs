\ compare.fs - Compare tf.fs native vs gcc -O2 on bench/*.fs
\ Usage: ./fifth bench/compare.fs
\ Shows compile time and runtime for both compilers.
\ Ratio = tf/gcc (lower is better for tf.fs, 1.00 = equal).

require lib/str.fs

: 0<> ( n -- flag ) 0= 0= ;

variable t0
variable t1
variable t2
variable t3

create cmd-buf 512 allot  variable cmd-len
: cmd-reset ( -- ) 0 cmd-len ! ;
: cmd+ ( addr u -- )
  dup cmd-len @ + 511 < if
    cmd-buf cmd-len @ + swap dup cmd-len +! move
  else 2drop then ;
: cmd$ ( -- addr u ) cmd-buf cmd-len @ ;

create name-buf 64 allot  variable name-len
: stash-name ( addr u -- ) dup name-len ! name-buf swap move ;
: name$ ( -- addr u ) name-buf name-len @ ;

: .name ( -- )
  name$ type
  8 name-len @ - dup 0> if spaces else drop then ;

: time1 ( addr u -- ms )
  clock-ms t0 !
  system-rc drop
  clock-ms t0 @ - ;

: time3 ( addr u -- ms )
  2dup time1 t1 !
  2dup time1 t2 !
  time1 t3 !
  t1 @ t2 @ t3 @
  >r 2dup < if swap then r>
  2dup < if swap then drop
  min ;

: .d ( n -- ) [char] 0 + emit ;

: .x100 ( n -- )
  dup 0< if [char] - emit negate then
  dup 9999 > if
    100 / . [char] x emit exit
  then
  dup 100 / .d
  [char] . emit
  dup 10 / 10 mod .d
  10 mod .d
  [char] x emit ;

: .ratio ( tf-ms gcc-ms -- )
  over 0= over 0= or if 2drop s" -" type exit then
  swap 100 * swap / .x100 ;

variable tf-comp
variable gcc-comp
variable tf-run
variable gcc-run

: run-bench ( addr u -- )
  stash-name

  \ --- Compile with tf.fs (median of 3) ---
  cmd-reset
  s" ./fifth compiler/tf.fs bench/" cmd+
  name$ cmd+
  s" .fs /tmp/_bench_cmp >/dev/null 2>&1" cmd+
  cmd$ time3 tf-comp !

  \ Check it compiled
  cmd-reset s" test -f /tmp/_bench_cmp" cmd+
  cmd$ system-rc
  0<> if
    ." | " .name ."  | SKIP | - | - | - | - | - |" cr
    exit
  then

  \ --- Compile with gcc -O2 (median of 3) ---
  cmd-reset
  s" gcc -O2 -o /tmp/_bench_gcc bench/bench.c >/dev/null 2>&1" cmd+
  cmd$ time3 gcc-comp !

  \ --- Run native (check crash first) ---
  cmd-reset
  s" timeout 2 /tmp/_bench_cmp >/dev/null 2>&1" cmd+
  clock-ms t0 !
  cmd$ system-rc
  clock-ms t0 @ - drop
  0<> if
    ." | " .name ."  | "
    tf-comp @ . ." ms | "
    gcc-comp @ . ." ms | "
    ." CRASH | - | - |" cr
    exit
  then

  \ Median of 3 runtime
  cmd-reset
  s" /tmp/_bench_cmp >/dev/null 2>&1" cmd+
  cmd$ time3 tf-run !

  \ --- Run gcc ---
  cmd-reset
  s" /tmp/_bench_gcc " cmd+
  name$ cmd+
  s"  >/dev/null 2>&1" cmd+
  cmd$ time3 gcc-run !

  \ --- Print row ---
  ." | " .name ."  | "
  tf-comp @ . ." ms | "
  gcc-comp @ . ." ms | "
  tf-run @ . ." ms | "
  gcc-run @ . ." ms | "
  tf-run @ gcc-run @ .ratio
  ."  |" cr ;

: main ( -- )
  s" rm -f /tmp/_bench_cmp /tmp/_bench_gcc" system
  cr
  ." | Bench    | tf comp | gcc comp | tf run | gcc run | tf/gcc |" cr
  ." |----------|---------|----------|--------|---------|--------|" cr

  s" arith"  run-bench
  s" loop"   run-bench
  s" branch" run-bench
  s" stack"  run-bench
  s" nested" run-bench
  s" fib"    run-bench
  s" mem"    run-bench
  s" call"   run-bench
  s" fibrec" run-bench
  cr ;

main bye
