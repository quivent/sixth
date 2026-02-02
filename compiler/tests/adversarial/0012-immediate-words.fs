\ Adversarial Test 0012: Immediate Words and Compilation
\ Compile-time vs run-time. The distinction that breaks everything.

: test-literal ( -- )
  [ 5 3 + ] literal 8 = if ." PASS" else ." FAIL" then cr ;

: test-bracket-tick ( -- )
  ['] dup execute 5 = -rot 5 = and swap 5 = and if ." PASS" else ." FAIL" then cr ;

5 test-bracket-tick

: test-postpone ( -- )
  : test-pp postpone dup ; immediate
  : use-pp test-pp ;
  5 use-pp + 10 = if ." PASS" else ." FAIL" then cr ;

: test-state ( -- )
  state @ 0= if ." PASS" else ." FAIL" then cr ;

: test-state-compile ( -- )
  : check-state state @ ;
  check-state 0= if ." PASS" else ." FAIL" then cr ;

: test-bracket-char ( -- )
  [char] X 88 = if ." PASS" else ." FAIL" then cr ;

: test-compile-comma ( -- )
  : make-dup ['] dup compile, ;
  : test-it make-dup ;
  5 test-it + 10 = if ." PASS" else ." FAIL" then cr ;

." 0012-immediate-words: " cr
." literal:   " test-literal
." state:     " test-state
." state-c:   " test-state-compile
." [char]:    " test-bracket-char

\ REMINDER: Immediate words execute during compilation. Get the timing wrong, chaos.
