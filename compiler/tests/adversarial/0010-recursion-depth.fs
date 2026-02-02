\ Adversarial Test 0010: Recursion
\ Tail calls. Deep stacks. Mutual recursion.

: factorial ( n -- n! )
  dup 1 <= if drop 1 else dup 1- recurse * then ;

: test-factorial-0 ( -- )
  0 factorial 1 = if ." PASS" else ." FAIL" then cr ;

: test-factorial-1 ( -- )
  1 factorial 1 = if ." PASS" else ." FAIL" then cr ;

: test-factorial-5 ( -- )
  5 factorial 120 = if ." PASS" else ." FAIL" then cr ;

: test-factorial-10 ( -- )
  10 factorial 3628800 = if ." PASS" else ." FAIL" then cr ;

: fib ( n -- fib[n] )
  dup 2 < if exit then
  dup 1- recurse swap 2 - recurse + ;

: test-fib-0 ( -- )
  0 fib 0 = if ." PASS" else ." FAIL" then cr ;

: test-fib-1 ( -- )
  1 fib 1 = if ." PASS" else ." FAIL" then cr ;

: test-fib-10 ( -- )
  10 fib 55 = if ." PASS" else ." FAIL" then cr ;

defer is-even?
defer is-odd?

:noname ( n -- flag )
  dup 0= if drop true else 1- is-odd? then ; is is-even?

:noname ( n -- flag )
  dup 0= if drop false else 1- is-even? then ; is is-odd?

: test-mutual-even ( -- )
  10 is-even? if ." PASS" else ." FAIL" then cr ;

: test-mutual-odd ( -- )
  11 is-odd? if ." PASS" else ." FAIL" then cr ;

." 0010-recursion-depth: " cr
." fact 0:    " test-factorial-0
." fact 1:    " test-factorial-1
." fact 5:    " test-factorial-5
." fact 10:   " test-factorial-10
." fib 0:     " test-fib-0
." fib 1:     " test-fib-1
." fib 10:    " test-fib-10
." mutual-e:  " test-mutual-even
." mutual-o:  " test-mutual-odd

\ REMINDER: Recursion stresses the return stack. Mutual recursion stresses the linker.
