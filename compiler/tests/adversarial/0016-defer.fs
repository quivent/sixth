\ Adversarial Test 0016: Deferred Words
\ Late binding in Forth.

defer my-action

: action1 ( -- n ) 1 ;
: action2 ( -- n ) 2 ;

' action1 is my-action

: test-defer-initial ( -- )
  my-action 1 = if ." PASS" else ." FAIL" then cr ;

: test-defer-change ( -- )
  ' action2 is my-action
  my-action 2 = if ." PASS" else ." FAIL" then cr ;

defer recur-defer

: count-down ( n -- )
  dup 0> if 1- recur-defer else drop then ;

' count-down is recur-defer

: test-defer-recurse ( -- )
  5 recur-defer ." PASS" cr ;

: test-action-of ( -- )
  action-of my-action ['] action2 = if ." PASS" else ." FAIL" then cr ;

." 0016-defer: " cr
." initial:   " test-defer-initial
." change:    " test-defer-change
." recurse:   " test-defer-recurse
." action-of: " test-action-of

\ REMINDER: DEFER is late binding. IS changes what runs. Powerful for polymorphism.
