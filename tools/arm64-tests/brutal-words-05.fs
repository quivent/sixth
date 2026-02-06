\ expect: 0
\ Test: Direct recursion - factorial
\ Tests self-referencing word

: factorial ( n -- n! )
  dup 1 <= if drop 1 else dup 1 - factorial * then ;

: check ( -- n )
  0 factorial 1 <> if 1 exit then
  1 factorial 1 <> if 2 exit then
  5 factorial 120 <> if 3 exit then
  6 factorial 720 <> if 4 exit then
  0 ;

: main check ;
