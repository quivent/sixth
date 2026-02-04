\ tools/monitor.fs - Fifth Compiler Monitor TUI
\ Usage: ./engine/fifth tools/monitor.fs

require lib/tui.fs
require tools/vocab.fs

\ Missing from interpreter, define here
: >= ( a b -- flag ) < 0= ;
: <= ( a b -- flag ) > 0= ;

\ ============================================================
\ TAB NAMES
\ ============================================================

: setup-tabs ( -- )
  s" Vocab"    0 tab-name!
  s" Compiler" 1 tab-name!
  s" Optimize" 2 tab-name!
  s" Tests"    3 tab-name!
  s" Bench"    4 tab-name!
  s" Files"    5 tab-name! ;

\ ============================================================
\ SHARED HELPERS
\ ============================================================

\ Pad with spaces to exact width
: pad-w ( n -- )
  0 max 0 ?do space loop ;

\ Type string, truncated to width, then pad
: type-pad ( addr u width -- )
  >r 2dup r@ min type
  r> swap - pad-w drop ;

\ Draw row number at left margin
: row-start ( row -- )
  3 + 1 term-goto ;

\ How many content rows available
: max-rows ( -- n ) term-rows @ 4 - ;



\ ============================================================
\ TAB 1: VOCABULARY BROWSER
\ ============================================================

variable vocab-filter  -1 vocab-filter !
variable vocab-comp-filter  -1 vocab-comp-filter !
variable vocab-cursor  0 vocab-cursor !

\ Implementation viewer state
create impl-buf 4096 allot
variable impl-len   0 impl-len !
variable impl-show  0 impl-show !

: should-show? ( i -- flag )
  dup vcat vocab-filter @ -1 = if drop true else vocab-filter @ = then
  swap vcomp vocab-comp-filter @ -1 = if drop true else vocab-comp-filter @ = then
  and ;

\ Count visible entries
: visible-count ( -- n )
  0 vocab-count @ 0 ?do
    i should-show? if 1+ then
  loop ;

\ Map cursor position to vocab entry index
: cursor-to-entry ( -- n )
  0 vocab-count @ 0 ?do
    i should-show? if
      dup vocab-cursor @ = if drop i unloop exit then
      1+
    then
  loop drop -1 ;

: draw-vocab-header ( -- )
  3 1 term-goto
  attr-bold fg-cyan
  s" Name" 16 type-pad
  s" Cat" 10 type-pad
  s" Stack Effect" 22 type-pad
  s" Description" 24 type-pad
  s" Freq" 5 type-pad
  s" TF" 3 type-pad
  attr-reset ;

: draw-vocab-row ( entry screen-row is-cursor -- )
  >r row-start term-erase-line
  r@ if attr-rev then
  dup vname 16 type-pad
  dup vcat cat-name 10 type-pad
  dup veffect 22 type-pad
  dup vdesc 24 type-pad
  dup vfreq freq-name 5 type-pad
  vcomp if fg-green ." Y" else fg-red ." -" then
  r> if attr-reset then
  attr-reset ;

: draw-vocab-status ( -- )
  term-rows @ 1- 1 term-goto term-erase-line
  attr-dim
  ." [" vocab-total <# #s #> type ." w, "
  vocab-compiled <# #s #> type ." compiled, "
  visible-count <# #s #> type ." shown] "
  ." f:" vocab-filter @ -1 = if ." All" else vocab-filter @ cat-name type then
  ."  t:" vocab-comp-filter @ case
    -1 of ." All" endof
     0 of ." Remaining" endof
     1 of ." Compiled" endof
  endcase
  ."  Enter:View"
  attr-reset ;

: show-implementation ( -- )
  cursor-to-entry dup -1 = if drop exit then
  vname ( addr u )
  str-reset
  s" grep -n -B2 -A15 '\$" str+
  str+
  s"  str=' compiler/sixth.fs > /tmp/fifth-impl.txt 2>&1" str+
  str$ system
  s" /tmp/fifth-impl.txt" slurp-file
  dup 0> if
    dup 4095 min dup impl-len ! impl-buf swap move
    -1 impl-show !
  else 2drop then ;

: draw-implementation ( -- )
  3 1 term-goto attr-bold fg-yellow
  ." sixth.fs implementation (any key to return)" attr-reset
  5 1 term-goto
  impl-buf impl-len @ tui-type ;

: draw-vocab ( -- )
  impl-show @ if draw-implementation exit then
  draw-vocab-header
  \ Auto-scroll to keep cursor visible
  vocab-cursor @ scroll@ < if vocab-cursor @ scroll! then
  vocab-cursor @ scroll@ max-rows 2 - + >= if
    vocab-cursor @ max-rows 2 - - 0 max scroll!
  then
  0 \ visible index
  vocab-count @ 0 ?do
    i should-show? if
      dup scroll@ >= if
        dup scroll@ - max-rows 1- < if
          i over scroll@ - 1+
          2 pick vocab-cursor @ =
          draw-vocab-row
        then
      then
      1+
    then
  loop drop
  draw-vocab-status ;

: handle-vocab-key ( c -- )
  impl-show @ if 0 impl-show ! drop exit then
  dup 10 = over 13 = or if drop show-implementation exit then
  dup [char] f = if drop
    vocab-filter @ 1+
    dup NUM-CATS >= if drop -1 then
    vocab-filter !
    0 vocab-cursor ! 0 scroll!
  exit then
  dup [char] t = if drop
    vocab-comp-filter @ 1+ dup 1 > if drop -1 then vocab-comp-filter !
    0 vocab-cursor ! 0 scroll!
  exit then
  drop ;

\ Arrow handler for vocab cursor navigation
: vocab-arrow ( dir -- )
  0< if
    vocab-cursor @ 1- 0 max vocab-cursor !
  else
    vocab-cursor @ 1+ visible-count 1- 0 max min vocab-cursor !
  then ;

: custom-arrow ( dir -- )
  current-tab @ 0= if vocab-arrow else
    0< if scroll-up else 999 scroll-down then
  then ;

\ ============================================================
\ TAB 2: COMPILER MODULE PROGRESS
\ ============================================================

\ Compiler sections - hardcoded from sixth.fs analysis
\ Each: name, description, line range

: draw-compiler-section ( row name-a name-u desc-a desc-u compiled total -- )
  2>r 2>r 2>r
  row-start term-erase-line
  2r> 20 type-pad
  2r> 30 type-pad
  2r> fg-cyan
  over <# #s #> type ." /" dup <# #s #> type space
  20 progress-bar
  attr-reset ;

: draw-compiler ( -- )
  3 1 term-goto
  attr-bold fg-cyan
  s" Category" 20 type-pad
  s" Description" 30 type-pad
  s" Coverage" 20 type-pad
  attr-reset

  1 s" Stack Ops" s" dup swap over rot nip tuck"
    CAT-STACK vocab-cat-compiled CAT-STACK vocab-cat-total
    draw-compiler-section
  2 s" Return Stack" s" >r r> r@ 2>r 2r> 2r@"
    CAT-RSTACK vocab-cat-compiled CAT-RSTACK vocab-cat-total
    draw-compiler-section
  3 s" Arithmetic" s" + - * / mod negate abs"
    CAT-ARITH vocab-cat-compiled CAT-ARITH vocab-cat-total
    draw-compiler-section
  4 s" Comparison" s" = <> < > 0= 0< 0>"
    CAT-CMP vocab-cat-compiled CAT-CMP vocab-cat-total
    draw-compiler-section
  5 s" Logic/Bitwise" s" and or xor invert lshift"
    CAT-LOGIC vocab-cat-compiled CAT-LOGIC vocab-cat-total
    draw-compiler-section
  6 s" Memory" s" @ ! c@ c! +! cells"
    CAT-MEM vocab-cat-compiled CAT-MEM vocab-cat-total
    draw-compiler-section
  7 s" I/O" s" . emit type cr"
    CAT-IO vocab-cat-compiled CAT-IO vocab-cat-total
    draw-compiler-section
  8 s" Control Flow" s" if/then begin/while do/loop"
    CAT-CTRL vocab-cat-compiled CAT-CTRL vocab-cat-total
    draw-compiler-section
  9 s" Strings" s\" s\" .\" [char]"
    CAT-STR vocab-cat-compiled CAT-STR vocab-cat-total
    draw-compiler-section
  10 s" Meta" s" : ; create does> recurse"
    CAT-META vocab-cat-compiled CAT-META vocab-cat-total
    draw-compiler-section
  11 s" Numeric Out" s" <# # #s #> hold sign"
    CAT-NUM vocab-cat-compiled CAT-NUM vocab-cat-total
    draw-compiler-section
  12 s" Misc" s" noop true false bl abort"
    CAT-MISC vocab-cat-compiled CAT-MISC vocab-cat-total
    draw-compiler-section

  \ Overall totals
  17 row-start term-erase-line
  attr-bold fg-yellow
  s" TOTAL" 20 type-pad
  s" All interpreter words" 30 type-pad
  vocab-compiled vocab-total
  20 progress-bar
  attr-reset

  \ Compiler-only extensions note
  19 row-start
  attr-dim
  ." Compiler-only words (not in interpreter): nos+ tuck+ 2+ 2- dup2 nzloop 1-nzloop"
  attr-reset
  20 row-start
  attr-dim
  ." Compiler-only builtins: <if >if =if 0<if 0=if 0=until"
  attr-reset ;

: handle-compiler-key ( c -- ) drop ;

\ ============================================================
\ TAB 3: OPTIMIZATION REGISTRY
\ ============================================================

\ Results buffer for optimization tests
create opt-results 2048 allot
variable opt-results-len  0 opt-results-len !
variable opt-tested  0 opt-tested !

variable _opt-row
: draw-opt-entry ( row name-a name-u tests-a tests-u desc-a desc-u -- )
  2swap 2>r 2>r 2>r
  dup _opt-row !
  row-start term-erase-line
  attr-bold fg-green ." [LOCKED] " attr-reset
  2r> type space
  attr-dim 2r> type attr-reset
  _opt-row @ 1+ row-start
  ."           Tests: " 2r> type ;

: draw-optim ( -- )
  3 1 term-goto attr-bold fg-yellow
  ." Optimization Registry - These are LOCKED. Do not modify without full test pass."
  attr-reset

  2 s" 1. CONSTANT FOLDING"
    s" 1000-1015,1027-1029,1035-1037,1042,1044-1046,1049"
    s" Evaluate literal arithmetic at compile time"
    draw-opt-entry

  5 s" 2. LITERAL-OP FUSION"
    s" 1016-1021,1034,1043,1047"
    s" Fuse literal + runtime op into one x86 insn"
    draw-opt-entry

  8 s" 3. DOUBLE PASS"
    s" 1030-1033,1048"
    s" Scan source first for forward references"
    draw-opt-entry

  11 s" 4. DEAD CODE ELIMINATION"
    s" (implicit)"
    s" Pure void words tracked for elimination"
    draw-opt-entry

  14 s" 5. TAIL-CALL OPTIMIZATION"
    s" (implicit)"
    s" recurse at end of def becomes jmp"
    draw-opt-entry

  17 s" 6. REGISTER-BASED STACK"
    s" (all benchmarks)"
    s" Depth<=3 lives in rax/rbx/rcx, no memory"
    draw-opt-entry

  \ Show test results if available
  opt-tested @ if
    21 row-start
    attr-bold
    ." Last check: " opt-results opt-results-len @ tui-type
    attr-reset
  then

  term-rows @ 2 - 1 term-goto
  attr-dim ." Press 'r' to run optimization tests" attr-reset ;

: run-opt-tests ( -- )
  s" ./engine/fifth compiler/tests/run.fs > /tmp/fifth-mon-opt.txt 2>&1" system
  s" /tmp/fifth-mon-opt.txt" slurp-file
  dup 0> 0= if 2drop exit then
  dup 2047 > if 2drop exit then
  dup opt-results-len ! opt-results swap move
  -1 opt-tested ! ;

: handle-optim-key ( c -- )
  [char] r = if run-opt-tests then ;

\ ============================================================
\ TAB 4: TEST MANAGER
\ ============================================================

create test-output 4096 allot
variable test-output-len  0 test-output-len !
variable test-loaded  0 test-loaded !

\ Test category display
: draw-tests ( -- )
  3 1 term-goto attr-bold fg-cyan
  ." Test Suite Status"
  attr-reset

  5 1 term-goto
  ." Categories:"
  6 1 term-goto ."   001-099:  Basic primitives (lit, stack, arith)"
  7 1 term-goto ."   100-999:  Intermediate (combinations, interactions)"
  8 1 term-goto ."   1000-1049: Optimization tests (fold, fuse, fwd-ref)"
  9 1 term-goto ."   1100-1199: Return stack operations"
  10 1 term-goto ."   1200-1299: Memory, variables, strings"
  11 1 term-goto ."   1300+:     Complex programs and stress tests"

  13 1 term-goto
  attr-bold ." Results:" attr-reset

  test-loaded @ if
    15 1 term-goto
    test-output test-output-len @ tui-type
  else
    15 1 term-goto
    attr-dim ." No results loaded. Press 'R' to run full test suite." attr-reset
    16 1 term-goto
    attr-dim ." Press 'r' to load last results from disk." attr-reset
  then

  term-rows @ 2 - 1 term-goto
  attr-dim ." R:Run all  r:Load last results" attr-reset ;

: load-test-results ( -- )
  s" /tmp/fifth-mon-testout.txt" slurp-file
  dup 0> if
    dup 4095 > if 2drop exit then
    dup test-output-len ! test-output swap move
    -1 test-loaded !
  else 2drop then ;

: run-all-tests ( -- )
  s" ./engine/fifth compiler/tests/run.fs > /tmp/fifth-mon-testout.txt 2>&1" system
  load-test-results ;

: handle-tests-key ( c -- )
  dup [char] R = if drop run-all-tests exit then
  [char] r = if load-test-results then ;

\ ============================================================
\ TAB 5: BENCHMARK RUNNER
\ ============================================================

create bench-output 8192 allot
variable bench-output-len  0 bench-output-len !
variable bench-loaded  0 bench-loaded !

: draw-bench ( -- )
  3 1 term-goto attr-bold fg-cyan
  ." Benchmark: sixth.fs vs gcc (-O0/-O1/-O2/-O3)"
  attr-reset

  5 1 term-goto
  ." 17 benchmarks: arith, arith-std, loop, loop-std, fib, fib-std,"
  6 1 term-goto
  ." branch, stack, nested, mem, call, collatz, spill, arith50m,"
  7 1 term-goto
  ." call100m, fib38, nested100k"

  9 1 term-goto attr-bold ." Results:" attr-reset

  bench-loaded @ if
    11 1 term-goto
    bench-output bench-output-len @
    max-rows 4 - 80 * min
    tui-type
  else
    11 1 term-goto
    attr-dim ." No results. Press 'r' to run benchmarks (takes minutes)." attr-reset
    12 1 term-goto
    attr-dim ." Press 'l' to load last bench/BENCHMARKS.md" attr-reset
  then

  term-rows @ 2 - 1 term-goto
  attr-dim ." r:Run benchmarks  l:Load BENCHMARKS.md" attr-reset ;

: load-bench-md ( -- )
  s" bench/BENCHMARKS.md" slurp-file
  dup 0> if
    dup 8191 > if 2drop exit then
    dup bench-output-len ! bench-output swap move
    -1 bench-loaded !
  else 2drop then ;

: run-benchmarks ( -- )
  s" bash bench/run-full.sh > /tmp/fifth-mon-bench.txt 2>&1" system
  s" /tmp/fifth-mon-bench.txt" slurp-file
  dup 0> if
    dup 8191 > if 2drop exit then
    dup bench-output-len ! bench-output swap move
    -1 bench-loaded !
  else 2drop then ;

: handle-bench-key ( c -- )
  dup [char] r = if drop run-benchmarks exit then
  [char] l = if load-bench-md then ;

\ ============================================================
\ TAB 6: FILESYSTEM MAP
\ ============================================================

create tree-output 8192 allot
variable tree-output-len  0 tree-output-len !
variable tree-loaded  0 tree-loaded !

: refresh-tree ( -- )
  str-reset
  s" (echo '=== Engine ===' && wc -l engine/*.c engine/*.h engine/boot/*.fs 2>/dev/null;" str+
  s"  echo && echo '=== Compiler ===' && wc -l compiler/sixth.fs 2>/dev/null;" str+
  s"  echo && echo '=== Libraries ===' && wc -l lib/*.fs 2>/dev/null;" str+
  s"  echo && echo '=== Tools ===' && wc -l tools/*.fs 2>/dev/null;" str+
  s"  echo && echo '=== Examples ===' && ls examples/*.fs 2>/dev/null | wc -l | tr -d ' ';" str+
  s"  echo ' example files';" str+
  s"  echo && echo '=== Tests ===' && ls compiler/tests/[0-9]*.fs 2>/dev/null | wc -l | tr -d ' ';" str+
  s"  echo ' test files';" str+
  s"  echo && echo '=== Benchmarks ===' && ls bench/*.fs 2>/dev/null | wc -l | tr -d ' ';" str+
  s"  echo ' benchmark files'" str+
  s" ) > /tmp/fifth-mon-tree.txt 2>&1" str+
  str$ system
  s" /tmp/fifth-mon-tree.txt" slurp-file
  dup 0> if
    dup 8191 > if 2drop exit then
    dup tree-output-len ! tree-output swap move
    -1 tree-loaded !
  else 2drop then ;

: draw-tree ( -- )
  3 1 term-goto attr-bold fg-cyan
  ." Project Filesystem Map"
  attr-reset

  tree-loaded @ 0= if refresh-tree then

  tree-loaded @ if
    5 1 term-goto
    tree-output tree-output-len @
    max-rows 3 - 80 * min
    tui-type
  else
    5 1 term-goto
    attr-dim ." Press 'r' to scan filesystem" attr-reset
  then

  term-rows @ 2 - 1 term-goto
  attr-dim ." r:Refresh" attr-reset ;

: handle-tree-key ( c -- )
  [char] r = if refresh-tree then ;

\ ============================================================
\ TAB DISPATCH
\ ============================================================

: draw-content ( -- )
  current-tab @ case
    0 of draw-vocab    endof
    1 of draw-compiler endof
    2 of draw-optim    endof
    3 of draw-tests    endof
    4 of draw-bench    endof
    5 of draw-tree     endof
  endcase ;

: handle-tab-key ( c -- )
  current-tab @ case
    0 of handle-vocab-key    endof
    1 of handle-compiler-key endof
    2 of handle-optim-key    endof
    3 of handle-tests-key    endof
    4 of handle-bench-key    endof
    5 of handle-tree-key     endof
  endcase ;

\ ============================================================
\ MAIN
\ ============================================================

: monitor ( -- )
  setup-tabs
  ['] draw-content 'draw-content !
  ['] handle-tab-key 'handle-tab-key !
  ['] custom-arrow 'handle-up-down !
  tui-run ;

monitor
bye
