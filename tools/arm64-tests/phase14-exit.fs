\ Phase 14: exit - early return from word
\ expect: 50

: early ( n -- n )
  dup 5 > if drop 99 exit then
  dup 3 > if drop 50 exit then
;

: main
  4 early
;
