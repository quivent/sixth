\ expect: 0
\ Brutal Integration Test 02: Memoized Fibonacci
\ Tests: recursion, memory, conditionals, arithmetic

variable memo-base

: memo@ ( n -- val ) cells memo-base @ + @ ;
: memo! ( val n -- ) cells memo-base @ + ! ;

: init-memo ( -- )
  here memo-base !
  30 cells allot
  30 0 do -1 i memo! loop
  0 0 memo!
  1 1 memo! ;

: fib ( n -- result )
  dup 2 < if exit then
  dup memo@ dup -1 <> if nip exit then
  drop
  dup 1- fib
  over 2 - fib
  + dup rot memo! ;

: main
  init-memo
  0 fib 0 <> if 1 exit then
  1 fib 1 <> if 1 exit then
  10 fib 55 <> if 1 exit then
  15 fib 610 <> if 1 exit then
  0 ;
