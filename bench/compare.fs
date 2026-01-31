\ compare.fs - Compare tf.fs native vs gcc -O2 on bench/*.fs
\ Usage: ./fifth bench/compare.fs
\ Compiles each bench with tf.fs, times native vs bench_c (gcc -O2).
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

\ Pad name to 8 chars
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

\ Print single digit
: .d ( n -- ) [char] 0 + emit ;

\ Print N.NNx from value*100
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

variable native-ms
variable gcc-ms

: run-bench ( addr u -- )
  stash-name

  \ Compile with tf.fs
  cmd-reset
  s" ./fifth compiler/tf.fs bench/" cmd+
  name$ cmd+
  s" .fs /tmp/_bench_cmp >/dev/null 2>&1" cmd+
  cmd$ system-rc
  0<> if
    ." | " .name ."  | SKIP  | -     | -     |" cr
    exit
  then

  \ Time native (single run, check for crash via exit code)
  cmd-reset
  s" timeout 2 /tmp/_bench_cmp >/dev/null 2>&1" cmd+
  clock-ms t0 !
  cmd$ system-rc
  clock-ms t0 @ - native-ms !
  0<> if
    ." | " .name ."  | CRASH | -     | -     |" cr
    exit
  then

  \ Now median of 3 (we know it works)
  cmd-reset
  s" /tmp/_bench_cmp >/dev/null 2>&1" cmd+
  cmd$ time3 native-ms !

  \ Time gcc
  cmd-reset
  s" timeout 3 bench/bench_c " cmd+
  name$ cmd+
  s"  >/dev/null 2>&1" cmd+
  cmd$ time3 gcc-ms !

  \ Print row: | name | tf ms | gcc ms | tf/gcc ratio |
  ." | " .name ."  | "
  native-ms @ . ." ms | "
  gcc-ms @ . ." ms | "
  \ Ratio = native * 100 / gcc (100 = 1.00x, lower = tf wins)
  gcc-ms @ 0= native-ms @ 0= or if
    s" ~1.00x" type
  else
    native-ms @ 100 * gcc-ms @ / .x100
  then
  ."  |" cr ;

: ensure-gcc ( -- )
  cmd-reset s" test -f bench/bench_c" cmd+
  cmd$ system-rc
  0<> if
    s" gcc -O2 -o bench/bench_c bench/bench.c" system
    ." Compiled bench/bench_c" cr
  then ;

: main ( -- )
  ensure-gcc
  cr
  ." | Bench    | tf.fs  | gcc-O2 | tf/gcc |" cr
  ." |----------|--------|--------|--------|" cr

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
