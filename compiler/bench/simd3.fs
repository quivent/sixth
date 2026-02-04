\ expected: 9999
\ Min/max reduction - GCC vectorizes reductions

create arr 80000 allot

: init ( -- )
  10000 0 do
    10000 i - 1- i 8 * arr + !
  loop ;

: find-max ( -- n )
  0 10000 0 do
    i 8 * arr + @ max
  loop ;

: main init find-max . cr ;
