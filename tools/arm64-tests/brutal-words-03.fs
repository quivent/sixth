\ expect: 0
\ Test: Mutual recursion - two words calling each other
\ is-even and is-odd implement mutual recursion

: is-odd ( n -- flag ) dup 0= if drop 0 else 1 - is-even then ;
: is-even ( n -- flag ) dup 0= if drop 1 else 1 - is-odd then ;

: check-all ( -- n )
  6 is-even 1 <>   if 1 exit then
  7 is-even 0 <>   if 2 exit then
  7 is-odd 1 <>    if 3 exit then
  8 is-odd 0 <>    if 4 exit then
  0 ;

: main check-all ;
