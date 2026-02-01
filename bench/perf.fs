\ perf.fs - Performance regression test (pure Fifth)
\ 1. Compiles and runs all 1000 compiler tests, checks total time ceiling
\ 2. Each bench/*.fs with a BENCH header: median-of-3, 10% threshold
\
\ Usage: ./sixth bench/perf.fs

\ Inline string buffer
4096 constant str-max
create str-buf str-max allot
variable str-len
: str-reset ( -- ) 0 str-len ! ;
: str+ ( addr u -- )
  dup str-len @ + str-max < if
    str-buf str-len @ + swap dup str-len +! move
  else 2drop then ;
: str$ ( -- addr u ) str-buf str-len @ ;

: >= ( a b -- flag ) < 0= ;
: <= ( a b -- flag ) > 0= ;

\ ============================================================
\ COUNTERS
\ ============================================================
variable pass    0 pass !
variable fail    0 fail !
variable total   0 total !

\ ============================================================
\ SHARED HELPERS
\ ============================================================
variable t0
variable fi

create bname 64 allot
variable bname#
: stash-bname ( addr u -- ) dup bname# ! bname swap move ;
: bname$ ( -- addr u ) bname bname# @ ;

create bpath 256 allot
variable bpath#
: stash-bpath ( addr u -- ) dup bpath# ! bpath swap move ;
: bpath$ ( -- addr u ) bpath bpath# @ ;

variable last-slash
: path>base ( addr u -- base-addr base-u )
  0 last-slash !
  dup 0 do
    over i + c@ [char] / = if i 1+ last-slash ! then
  loop
  last-slash @ /string
  dup 3 >= if
    2dup + 3 - c@ [char] . = if 3 - then
  then ;

65536 constant LIST-MAX
create list-buf LIST-MAX allot
variable list-len

: find-nl ( addr u -- pos )
  0 begin
    2dup > while
    2 pick over + c@ 10 = if nip nip exit then
    1+
  repeat nip nip ;

\ ============================================================
\ PART 1: ALL 1000 TESTS (total time ceiling)
\ ============================================================

10000 constant TEST-CEIL-MS   \ ceiling for compile+run of all tests

variable test-pass   0 test-pass !
variable test-cfail  0 test-cfail !
variable test-rfail  0 test-rfail !
variable test-total  0 test-total !

: run-one-test ( path-addr path-u -- )
  2dup stash-bpath
  2drop
  bpath$ path>base stash-bname
  1 test-total +!
  \ Compile
  str-reset
  s" ./sixth compiler/sixth.fs " str+
  bpath$ str+
  s"  /tmp/t-" str+
  bname$ str+
  s"  >/dev/null 2>&1" str+
  str$ system-rc
  0 <> if 1 test-cfail +! exit then
  \ Run
  str-reset
  s" timeout 2 /tmp/t-" str+
  bname$ str+
  s"  >/dev/null 2>&1" str+
  str$ system-rc
  0 <> if 1 test-rfail +! else 1 test-pass +! then ;

: run-all-tests ( -- ms )
  s" ls compiler/tests/[0-9]*.fs > /tmp/perf-tlist.txt 2>/dev/null" system
  s" /tmp/perf-tlist.txt" slurp-file
  dup 0= if 2drop 0 exit then
  dup list-len ! list-buf swap move
  list-buf list-len @
  clock-ms t0 !
  begin
    dup 0> while
    2dup find-nl >r
    over r@
    dup 3 > if run-one-test else 2drop then
    r> 1+ /string
  repeat
  2drop
  clock-ms t0 @ - ;

: check-tests ( -- )
  ." === Compiler tests (1000) ===" cr
  run-all-tests
  1 total +!
  dup TEST-CEIL-MS 110 * 100 / > if
    ." FAIL " 1 fail +!
  else
    ." PASS " 1 pass +!
  then
  ." all-tests      "
  ."  total=" . ." ms/" TEST-CEIL-MS . ." ms"
  ."  (pass=" test-pass @ .
  ." cfail=" test-cfail @ .
  ." rfail=" test-rfail @ . ." )" cr ;

\ ============================================================
\ PART 2: BENCH HEADERS (individual benchmarks)
\ ============================================================

create hdr-buf 256 allot
variable hdr-len
variable compile-ceil  0 compile-ceil !
variable run-ceil      0 run-ceil !

: parse-val ( addr -- n )
  0 swap
  begin dup c@ dup [char] 0 >= over [char] 9 <= and while
    [char] 0 - rot 10 * + swap 1+
  repeat drop drop ;

: find-compile= ( -- addr | 0 )
  0 fi !
  begin
    fi @ hdr-len @ 8 - > if 0 exit then
    hdr-buf fi @ + c@ [char] c =
    hdr-buf fi @ 1+ + c@ [char] o = and
    hdr-buf fi @ 2 + + c@ [char] m = and
    hdr-buf fi @ 3 + + c@ [char] p = and
    hdr-buf fi @ 4 + + c@ [char] i = and
    hdr-buf fi @ 5 + + c@ [char] l = and
    hdr-buf fi @ 6 + + c@ [char] e = and
    hdr-buf fi @ 7 + + c@ [char] = = and
    if hdr-buf fi @ 8 + + exit then
    1 fi +!
  again ;

: find-run= ( -- addr | 0 )
  0 fi !
  begin
    fi @ hdr-len @ 4 - > if 0 exit then
    hdr-buf fi @ + c@ [char] r =
    hdr-buf fi @ 1+ + c@ [char] u = and
    hdr-buf fi @ 2 + + c@ [char] n = and
    hdr-buf fi @ 3 + + c@ [char] = = and
    if hdr-buf fi @ 4 + + exit then
    1 fi +!
  again ;

: has-bench? ( -- flag )
  0 fi !
  begin
    fi @ hdr-len @ 5 - > if false exit then
    hdr-buf fi @ + c@ [char] B =
    hdr-buf fi @ 1+ + c@ [char] E = and
    hdr-buf fi @ 2 + + c@ [char] N = and
    hdr-buf fi @ 3 + + c@ [char] C = and
    hdr-buf fi @ 4 + + c@ [char] H = and
    if true exit then
    1 fi +!
  again ;

: trim-to-line ( -- )
  0 fi !
  begin
    fi @ hdr-len @ >= if exit then
    hdr-buf fi @ + c@ 10 = if fi @ hdr-len ! exit then
    1 fi +!
  again ;

: read-header ( path-addr path-u -- ok? )
  slurp-file
  dup 0= if 2drop false exit then
  dup 255 min hdr-len !
  drop hdr-buf hdr-len @ move
  trim-to-line
  hdr-len @ 5 < if false exit then
  has-bench? 0= if false exit then
  find-compile= ?dup if parse-val compile-ceil ! else 0 compile-ceil ! then
  find-run= ?dup if parse-val run-ceil ! else 0 run-ceil ! then
  true ;

: median3 ( a b c -- median )
  >r 2dup < if swap then r>
  2dup < if swap then drop
  min ;

: time-compile ( -- ms )
  str-reset
  s" ./sixth compiler/sixth.fs " str+
  bpath$ str+
  s"  /tmp/perf-" str+
  bname$ str+
  s"  >/dev/null 2>&1" str+
  clock-ms t0 !
  str$ system-rc drop
  clock-ms t0 @ - ;

: time-run ( -- ms )
  str-reset
  s" /tmp/perf-" str+
  bname$ str+
  s"  >/dev/null 2>&1" str+
  clock-ms t0 !
  str$ system-rc drop
  clock-ms t0 @ - ;

: .padname ( addr u -- )
  dup 16 < if
    2dup type 16 swap - spaces drop
  else
    drop 16 type
  then ;

variable c-ms
variable r-ms
variable c-limit
variable r-limit

: run-bench ( path-addr path-u -- )
  2dup stash-bpath
  2dup read-header 0= if 2drop exit then
  2drop
  bpath$ path>base stash-bname
  1 total +!

  time-compile time-compile time-compile median3 c-ms !
  time-run time-run time-run median3 r-ms !

  compile-ceil @ 110 * 100 / c-limit !
  run-ceil @ 110 * 100 / r-limit !

  c-ms @ c-limit @ > r-ms @ r-limit @ > or if
    ." FAIL " 1 fail +!
  else
    ." PASS " 1 pass +!
  then

  bname$ .padname
  ."  compile=" c-ms @ . ." ms/" compile-ceil @ . ." ms"
  ."   run=" r-ms @ . ." ms/" run-ceil @ . ." ms"
  c-ms @ c-limit @ > if ."  (compile exceeded)" then
  r-ms @ r-limit @ > if ."  (run exceeded)" then
  cr ;

: check-benches ( -- )
  cr ." === Benchmarks ===" cr
  s" ls bench/*.fs > /tmp/perf-blist.txt 2>/dev/null" system
  s" /tmp/perf-blist.txt" slurp-file
  dup 0= if 2drop exit then
  dup list-len ! list-buf swap move
  list-buf list-len @
  begin
    dup 0> while
    2dup find-nl >r
    over r@
    dup 3 > if run-bench else 2drop then
    r> 1+ /string
  repeat
  2drop ;

\ ============================================================
\ MAIN
\ ============================================================

: main
  check-tests
  check-benches
  cr
  ." TOTAL: " total @ .
  ."  PASS: " pass @ .
  ."  FAIL: " fail @ . cr
  fail @ 0 <> if 1 throw then
  bye ;

main
