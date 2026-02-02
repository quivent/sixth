\ Adversarial Test 0020: CASE Statement
\ Multi-way branch. Each OF must work.

: day-name ( n -- addr u )
  case
    0 of s" Sunday" endof
    1 of s" Monday" endof
    2 of s" Tuesday" endof
    3 of s" Wednesday" endof
    4 of s" Thursday" endof
    5 of s" Friday" endof
    6 of s" Saturday" endof
    drop s" Unknown"
  endcase ;

: test-case-0 ( -- )
  0 day-name s" Sunday" compare 0= if ." PASS" else ." FAIL" then cr ;

: test-case-3 ( -- )
  3 day-name s" Wednesday" compare 0= if ." PASS" else ." FAIL" then cr ;

: test-case-6 ( -- )
  6 day-name s" Saturday" compare 0= if ." PASS" else ." FAIL" then cr ;

: test-case-default ( -- )
  99 day-name s" Unknown" compare 0= if ." PASS" else ." FAIL" then cr ;

: nested-case ( a b -- n )
  case
    1 of
      case
        10 of 100 endof
        20 of 200 endof
        0 swap
      endcase
    endof
    2 of
      case
        30 of 300 endof
        40 of 400 endof
        0 swap
      endcase
    endof
    drop 0
  endcase ;

: test-nested-case ( -- )
  10 1 nested-case 100 = if ." PASS" else ." FAIL" then cr ;

." 0020-case: " cr
." case 0:    " test-case-0
." case 3:    " test-case-3
." case 6:    " test-case-6
." default:   " test-case-default
." nested:    " test-nested-case

\ REMINDER: CASE consumes the selector. OF compares and jumps. ENDCASE cleans up.
