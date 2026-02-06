\ expect: 0
\ Brutal Integration Test 08: RPN Calculator
\ Tests: char handling, stack ops, dispatch

variable input-base
variable num-st-base
variable num-sp

: num-push ( n -- ) num-sp @ cells num-st-base @ + ! num-sp @ 1+ num-sp ! ;
: num-pop ( -- n ) num-sp @ 1- dup num-sp ! cells num-st-base @ + @ ;

: digit? ( c -- flag ) dup 48 >= swap 57 <= and ;

: eval-char ( c -- )
  dup digit? if 48 - num-push exit then
  dup 43 = if drop num-pop num-pop + num-push exit then
  dup 45 = if drop num-pop num-pop swap - num-push exit then
  dup 42 = if drop num-pop num-pop * num-push exit then
  drop ;

: eval-rpn ( addr len -- result )
  0 num-sp !
  over + swap
  begin
    2dup > while
    dup c@ eval-char
    1+
  repeat
  2drop
  num-pop ;

: set-ch ( c n -- ) input-base @ + c! ;

: init-eval ( -- )
  here input-base ! 32 allot
  here num-st-base ! 16 cells allot ;

: main
  init-eval
  \ "34+" = 7
  51 0 set-ch  52 1 set-ch  43 2 set-ch
  input-base @ 3 eval-rpn 7 <> if 1 exit then
  \ "52-" = 3
  53 0 set-ch  50 1 set-ch  45 2 set-ch
  input-base @ 3 eval-rpn 3 <> if 1 exit then
  \ "34*" = 12
  51 0 set-ch  52 1 set-ch  42 2 set-ch
  input-base @ 3 eval-rpn 12 <> if 1 exit then
  0 ;
