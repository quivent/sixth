\ expect: 42
\ Test: BEGIN-AGAIN infinite loop with internal IF-THEN that uses EXIT
\ EXIT from inside nested control must properly escape the entire word
\ Tests: emit-exit within nested control, stack cleanup on early exit

: find-first-even ( start -- n )
  \ Loop until we find an even number, then EXIT
  begin
    dup 2 mod 0= if
      exit              \ Early exit when even - should return current value
    then
    1+                  \ Increment and try again
  again                 \ Infinite loop (never reached if EXIT works)
;

: find-mult7 ( start -- n )
  \ More complex: nested IF inside the exit check
  begin
    dup 7 mod 0= if
      dup 0> if
        exit            \ EXIT from doubly-nested IF
      then
    then
    1+
  again
;

: countdown-exit ( n -- n )
  \ Test EXIT preserves stack correctly in loop
  begin
    dup 0= if
      1+                \ Convert 0 to 1 before exit
      exit
    then
    1-
  again
;

: main
  3 find-first-even     \ 3 is odd, 4 is even -> returns 4
  1 find-mult7          \ 1,2,3,4,5,6,7 -> returns 7
  5 countdown-exit      \ 5,4,3,2,1,0 -> exit with 1
  + +                   \ 4 + 7 + 1 = 12
  30 +                  \ 12 + 30 = 42
;
