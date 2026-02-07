\ expect: 42
\ Test: BEGIN-WHILE-REPEAT with WHILE condition that is IMMEDIATELY FALSE
\ This tests the zero-iteration case where the loop body is never executed
\ Critical edge case: gen-while must properly patch forward even when body skipped

: zero-iter-while ( n -- n )
  \ This loop should never execute because 0 is immediately false
  0
  begin
    dup 0>          \ false on first check
  while
    1+              \ never reached
  repeat
  drop
;

: also-zero ( -- n )
  10               \ start with 10
  begin
    dup 0<          \ 10 < 0 is false, exit immediately
  while
    99 +            \ never reached
  repeat
;

: chained-zeros ( -- n )
  \ Multiple zero-iteration loops in sequence
  5
  begin dup 100 > while 1+ repeat   \ false: 5 > 100
  begin dup 0 < while 1- repeat     \ false: 5 < 0
  begin dup 5 > while 1+ repeat     \ false: 5 > 5
;

: main
  0 zero-iter-while       \ should return 0
  also-zero               \ should return 10
  chained-zeros           \ should return 5
  + + 27 +                \ 0 + 10 + 5 + 27 = 42
;
