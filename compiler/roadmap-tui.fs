\ compiler/roadmap-tui.fs - Sovereignty Roadmap TUI
\ Shows the path from Linux dependency to bare metal
\
\ Usage: ./fifth compiler/roadmap-tui.fs

require lib/tui.fs

\ ============================================================
\ PHASE DATA
\ ============================================================

\ Status codes: 0=NOT STARTED, 1=PARTIAL, 2=DONE, 3=FUTURE
0 constant ST-TODO
1 constant ST-PARTIAL
2 constant ST-DONE
3 constant ST-FUTURE

\ Phase structure: status, lines, name-addr, name-len, elim-addr, elim-len
\ We'll use simple constants for clarity

10 constant NUM-PHASES

\ Phase data arrays
create phase-status  NUM-PHASES cells allot
create phase-lines   NUM-PHASES cells allot
create phase-names   NUM-PHASES 20 * allot
create phase-elims   NUM-PHASES 30 * allot

: phase-name! ( addr u n -- )
  20 * phase-names + swap dup 19 min >r swap r@ move r> swap c! ;

: phase-name@ ( n -- addr u )
  20 * phase-names + dup c@ swap 1+ swap ;

: phase-elim! ( addr u n -- )
  30 * phase-elims + swap dup 29 min >r swap r@ move r> swap c! ;

: phase-elim@ ( n -- addr u )
  30 * phase-elims + dup c@ swap 1+ swap ;

: init-phases ( -- )
  \ Phase 0: Parsing
  ST-DONE    phase-status 0 cells + !
  100        phase-lines 0 cells + !
  s" 1. Parsing"          0 phase-name!
  s" C lexer"             0 phase-elim!

  \ Phase 1: Dictionary
  ST-TODO    phase-status 1 cells + !
  50         phase-lines 1 cells + !
  s" 2. Dictionary"       1 phase-name!
  s" C symbol table"      1 phase-elim!

  \ Phase 2: State
  ST-TODO    phase-status 2 cells + !
  35         phase-lines 2 cells + !
  s" 3. State"            2 phase-name!
  s" C state machine"     2 phase-elim!

  \ Phase 3: DOES>
  ST-TODO    phase-status 3 cells + !
  60         phase-lines 3 cells + !
  s" 4. DOES>"            3 phase-name!
  s" C defining words"    3 phase-elim!

  \ Phase 4: Interpreter
  ST-TODO    phase-status 4 cells + !
  110        phase-lines 4 cells + !
  s" 5. Interpreter"      4 phase-name!
  s" C REPL"              4 phase-elim!

  \ Phase 5: File I/O
  ST-PARTIAL phase-status 5 cells + !
  70         phase-lines 5 cells + !
  s" 6. File I/O"         5 phase-name!
  s" cat, pipes, heredocs" 5 phase-elim!

  \ Phase 6: Cleanup
  ST-TODO    phase-status 6 cells + !
  0          phase-lines 6 cells + !
  s" 7. Cleanup"          6 phase-name!
  s" GCC DEPENDENCY"      6 phase-elim!

  \ Phase 7: Native SQLite
  ST-TODO    phase-status 7 cells + !
  100        phase-lines 7 cells + !
  s" 8. Native SQLite"    7 phase-name!
  s" sqlite3 shell-out"   7 phase-elim!

  \ Phase 8: Test framework
  ST-TODO    phase-status 8 cells + !
  80         phase-lines 8 cells + !
  s" 9. Test framework"   8 phase-name!
  s" bash test runner"    8 phase-elim!

  \ Phase 9: Bare metal
  ST-FUTURE  phase-status 9 cells + !
  -15        phase-lines 9 cells + !
  s" 10. Bare metal"      9 phase-name!
  s" LINUX KERNEL"        9 phase-elim!
;

\ ============================================================
\ STATUS HELPERS
\ ============================================================

: status-color ( status -- )
  case
    ST-DONE    of fg-green  endof
    ST-PARTIAL of fg-yellow endof
    ST-TODO    of fg-red    endof
    ST-FUTURE  of fg-cyan   endof
  endcase ;

: status-text ( status -- addr u )
  case
    ST-DONE    of s" DONE"        endof
    ST-PARTIAL of s" PARTIAL"     endof
    ST-TODO    of s" NOT STARTED" endof
    ST-FUTURE  of s" FUTURE"      endof
  endcase ;

\ ============================================================
\ CALCULATE TOTALS
\ ============================================================

variable total-lines
variable done-lines
variable done-count

: calc-totals ( -- )
  0 total-lines !
  0 done-lines !
  0 done-count !
  NUM-PHASES 0 do
    phase-lines i cells + @
    dup 0> if total-lines +! else drop then
    phase-status i cells + @ ST-DONE = if
      phase-lines i cells + @ done-lines +!
      1 done-count +!
    then
  loop ;

\ ============================================================
\ TAB 1: THE LAW
\ ============================================================

: draw-law ( row -- row' )
  dup 1 term-goto
  attr-bold fg-magenta
  ." THE LAW: Sixth depends on nothing." attr-reset tui-cr

  1+ dup 1 term-goto tui-cr
  1+ dup 1 term-goto
  ." No bash. No shell scripts. No piping. No heredocs." tui-cr

  1+ dup 1 term-goto tui-cr
  1+ dup 1 term-goto
  fg-cyan
  ." Every external dependency is a failure." attr-reset tui-cr

  1+ dup 1 term-goto
  fg-cyan
  ." Every line of bash is an admission that Forth is incomplete." attr-reset tui-cr

  1+ dup 1 term-goto tui-cr
  1+ dup 1 term-goto
  ." The goal is not a small line count - the goal is "
  attr-bold fg-green ." sovereignty." attr-reset tui-cr

  1+ dup 1 term-goto tui-cr
  1+ dup 1 term-goto tui-cr
  1+ dup 1 term-goto
  attr-bold ." COMPLEXITY ANALYSIS" attr-reset tui-cr

  1+ dup 1 term-goto tui-cr
  1+ dup 1 term-goto
  ." The compiler was the hard part. Everything else is easier." tui-cr

  1+ dup 1 term-goto tui-cr
  1+ dup 1 term-goto
  fg-yellow ." Phase     Lines   % of compiler   Nature" attr-reset tui-cr

  1+ dup 1 term-goto
  ." No C      ~325    12%             Last intellectual work" tui-cr

  1+ dup 1 term-goto
  ." No bash   ~180    6%              Plumbing, not insight" tui-cr

  1+ dup 1 term-goto
  fg-green
  ." No Linux  ~5      0.2%            REMOVAL (colorForth exists)" attr-reset tui-cr

  1+ dup 1 term-goto tui-cr
  1+ dup 1 term-goto
  attr-bold
  ." The path to bare metal is shorter than the path already walked."
  attr-reset tui-cr
  1+
;

\ ============================================================
\ TAB 2: PHASES
\ ============================================================

: draw-phase-header ( row -- row' )
  dup 1 term-goto
  attr-bold
  ." Phase              Lines  Eliminates                Status" attr-reset tui-cr
  1+ dup 1 term-goto
  fg-cyan
  60 0 do [char] - emit loop
  attr-reset tui-cr 1+
;

: draw-phase ( n row -- row' )
  swap >r
  dup 1 term-goto

  \ Name (18 chars)
  r@ phase-name@ tui-type
  18 r@ phase-name@ nip - 0 max spaces

  \ Lines (6 chars)
  r@ cells phase-lines + @ dup 0< if
    fg-green ." -" negate emit-num attr-reset
  else
    emit-num
  then
  6 r@ cells phase-lines + @ abs
  dup 100 < if 1+ then
  dup 10 < if 1+ then
  - spaces

  \ Eliminates (26 chars)
  r@ phase-elim@ tui-type
  26 r@ phase-elim@ nip - 0 max spaces

  \ Status
  r@ cells phase-status + @
  dup status-color
  status-text tui-type
  attr-reset

  r> drop
  tui-cr 1+
;

: draw-phases ( row -- row' )
  draw-phase-header
  NUM-PHASES 0 do
    i swap draw-phase
  loop
;

\ ============================================================
\ TAB 3: MILESTONES
\ ============================================================

: draw-milestones ( row -- row' )
  dup 1 term-goto
  attr-bold ." DEPENDENCY REDUCTION PATH" attr-reset tui-cr

  1+ dup 1 term-goto tui-cr
  1+ dup 1 term-goto
  fg-yellow ." Milestone         Lines   Depends On" attr-reset tui-cr

  1+ dup 1 term-goto
  fg-cyan 40 0 do [char] - emit loop attr-reset tui-cr

  1+ dup 1 term-goto
  ." Current           2800    " fg-red ." C, bash, Linux" attr-reset tui-cr

  1+ dup 1 term-goto
  ." After Phase 7     ~3125   " fg-yellow ." bash, Linux" attr-reset tui-cr

  1+ dup 1 term-goto
  ." After Phase 9     ~3305   " fg-cyan ." Linux" attr-reset tui-cr

  1+ dup 1 term-goto
  attr-bold
  ." After Phase 10    ~3290   " fg-green ." NOTHING" attr-reset tui-cr

  1+ dup 1 term-goto tui-cr
  1+ dup 1 term-goto tui-cr
  1+ dup 1 term-goto
  attr-bold ." PROGRESS" attr-reset tui-cr

  1+ dup 1 term-goto tui-cr
  1+ dup 1 term-goto
  ." Phases: "
  done-count @ emit-num ." /" NUM-PHASES emit-num ."  "
  done-count @ NUM-PHASES 30 progress-bar tui-cr

  1+ dup 1 term-goto tui-cr
  1+ dup 1 term-goto
  ." Lines:  "
  done-lines @ emit-num ." /" total-lines @ emit-num ."  "
  done-lines @ total-lines @ 30 progress-bar tui-cr

  1+ dup 1 term-goto tui-cr
  1+ dup 1 term-goto tui-cr
  1+ dup 1 term-goto
  attr-bold ." COMPARISON" attr-reset tui-cr

  1+ dup 1 term-goto tui-cr
  1+ dup 1 term-goto
  fg-red   ." GCC:    15,000,000 lines + glibc + binutils + make" attr-reset tui-cr
  1+ dup 1 term-goto
  fg-red   ." LLVM:   20,000,000 lines + CMake + Python" attr-reset tui-cr
  1+ dup 1 term-goto
  fg-green ." Sixth:  3,290 lines + nothing" attr-reset tui-cr

  1+ dup 1 term-goto tui-cr
  1+ dup 1 term-goto
  ." Ratio: " attr-bold fg-green ." 6000:1" attr-reset tui-cr
  1+
;

\ ============================================================
\ TAB 4: SUCCESS CRITERIA
\ ============================================================

: draw-criteria ( row -- row' )
  dup 1 term-goto
  attr-bold ." SOVEREIGNTY COMPLETE WHEN:" attr-reset tui-cr

  1+ dup 1 term-goto tui-cr
  1+ dup 1 term-goto
  fg-yellow ." 1. " attr-reset ." ./sixth sixth.fs produces working binary" tui-cr
  1+ dup 1 term-goto
  fg-yellow ." 2. " attr-reset ." That binary can compile sixth.fs again (bootstrap)" tui-cr
  1+ dup 1 term-goto
  fg-yellow ." 3. " attr-reset ." ./sixth starts interactive REPL" tui-cr
  1+ dup 1 term-goto
  fg-yellow ." 4. " attr-reset ." rm -rf engine/ - no C required" tui-cr
  1+ dup 1 term-goto
  fg-yellow ." 5. " attr-reset ." Hayes Core tests pass" tui-cr
  1+ dup 1 term-goto
  fg-yellow ." 6. " attr-reset ." Zero shell-outs in any Sixth program" tui-cr

  1+ dup 1 term-goto tui-cr
  1+ dup 1 term-goto
  attr-bold ." ULTIMATE SOVEREIGNTY (Phase 10):" attr-reset tui-cr

  1+ dup 1 term-goto tui-cr
  1+ dup 1 term-goto
  fg-green ." 7. " attr-reset ." Boots from BIOS - no Linux" tui-cr
  1+ dup 1 term-goto
  fg-green ." 8. " attr-reset ." ~3000 lines from power-on to native code" tui-cr
  1+ dup 1 term-goto
  fg-green ." 9. " attr-reset ." Depends on nothing but electricity" tui-cr

  1+ dup 1 term-goto tui-cr
  1+ dup 1 term-goto tui-cr
  1+ dup 1 term-goto
  attr-bold fg-cyan
  ." Chuck Moore solved this in 2001 with colorForth."
  attr-reset tui-cr
  1+ dup 1 term-goto
  ." We just haven't listened yet." tui-cr
  1+
;

\ ============================================================
\ MAIN DRAW DISPATCH
\ ============================================================

: draw-content ( -- )
  3 \ start at row 3 (after tabs and separator)
  current-tab @ case
    0 of draw-law       endof
    1 of draw-phases    endof
    2 of draw-milestones endof
    3 of draw-criteria  endof
  endcase
  drop
;

\ ============================================================
\ SETUP AND MAIN
\ ============================================================

: setup-tabs ( -- )
  s" The Law"     0 tab-name!
  s" Phases"      1 tab-name!
  s" Milestones"  2 tab-name!
  s" Success"     3 tab-name!
  s" "            4 tab-name!
  s" "            5 tab-name!
;

: main ( -- )
  init-phases
  calc-totals
  setup-tabs
  ['] draw-content 'draw-content !
  s"  Sixth Sovereignty Roadmap | q:Quit 1-4:Tabs" set-status
  tui-run
;

main bye
