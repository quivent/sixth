\ Adversarial Test 0004: Control Flow
\ Nested. Chained. Every branch path must work.

variable result

: test-if-true ( -- )
  1 if 1 else 0 then 1 = if ." PASS" else ." FAIL" then cr ;

: test-if-false ( -- )
  0 if 1 else 0 then 0 = if ." PASS" else ." FAIL" then cr ;

: test-nested-if ( -- )
  1 if
    1 if 1 else 0 then
  else
    0
  then
  1 = if ." PASS" else ." FAIL" then cr ;

: test-nested-if-inner-false ( -- )
  1 if
    0 if 1 else 2 then
  else
    0
  then
  2 = if ." PASS" else ." FAIL" then cr ;

: test-nested-if-outer-false ( -- )
  0 if
    1 if 1 else 2 then
  else
    3
  then
  3 = if ." PASS" else ." FAIL" then cr ;

: test-begin-until ( -- )
  0 result !
  0 begin 1+ dup 5 = until
  5 = if ." PASS" else ." FAIL" then cr ;

: test-begin-while-repeat ( -- )
  0
  begin dup 5 < while 1+ repeat
  5 = if ." PASS" else ." FAIL" then cr ;

: test-while-never-enters ( -- )
  0
  begin dup 0 < while 1+ repeat
  0 = if ." PASS" else ." FAIL" then cr ;

: test-do-loop ( -- )
  0
  5 0 do 1+ loop
  5 = if ." PASS" else ." FAIL" then cr ;

: test-do-loop-index ( -- )
  0
  5 0 do i + loop
  10 = if ." PASS" else ." FAIL" then cr ;

: test-do-plus-loop ( -- )
  0
  10 0 do 1+ 2 +loop
  5 = if ." PASS" else ." FAIL" then cr ;

: test-nested-do-loop ( -- )
  0
  3 0 do
    3 0 do 1+ loop
  loop
  9 = if ." PASS" else ." FAIL" then cr ;

." 0004-control-flow: " cr
." if-true:        " test-if-true
." if-false:       " test-if-false
." nested-if:      " test-nested-if
." nested-inner-f: " test-nested-if-inner-false
." nested-outer-f: " test-nested-if-outer-false
." begin-until:    " test-begin-until
." begin-while:    " test-begin-while-repeat
." while-skip:     " test-while-never-enters
." do-loop:        " test-do-loop
." do-loop-i:      " test-do-loop-index
." do-+loop:       " test-do-plus-loop
." nested-do:      " test-nested-do-loop

\ REMINDER: Standard control flow. Any Forth must handle this.
