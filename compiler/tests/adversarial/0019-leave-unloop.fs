\ Adversarial Test 0019: Loop Control - LEAVE and UNLOOP
\ Early exit from loops. Return stack cleanup.

: test-leave-basic ( -- )
  0
  10 0 do
    i 5 = if leave then
    1+
  loop
  5 = if ." PASS" else ." FAIL" then cr ;

: test-leave-value ( -- )
  10 0 do
    i 7 = if i leave then
  loop
  7 = if ." PASS" else ." FAIL" then cr ;

: test-leave-nested-inner ( -- )
  0
  3 0 do
    5 0 do
      i 2 = if leave then
      1+
    loop
  loop
  6 = if ." PASS" else ." FAIL" then cr ;

: find-in-loop ( n -- index|-1 )
  10 0 do
    dup i = if drop i unloop exit then
  loop
  drop -1 ;

: test-unloop-exit ( -- )
  5 find-in-loop 5 = if ." PASS" else ." FAIL" then cr ;

: test-unloop-not-found ( -- )
  99 find-in-loop -1 = if ." PASS" else ." FAIL" then cr ;

: test-leave-begin ( -- )
  0
  begin
    1+ dup 5 = if leave then
  dup 10 = until
  5 = if ." PASS" else ." FAIL" then cr ;

." 0019-leave-unloop: " cr
." leave:     " test-leave-basic
." leave val: " test-leave-value
." nested:    " test-leave-nested-inner
." unloop:    " test-unloop-exit
." not found: " test-unloop-not-found

\ REMINDER: LEAVE exits the innermost DO loop. UNLOOP cleans up before EXIT.
