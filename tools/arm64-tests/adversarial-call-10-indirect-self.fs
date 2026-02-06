\ expect: 3
\ Word calling itself indirectly through another word
\ indirect-dec calls itself via trampoline
\ 3 -> 2 -> 1 -> 0 exit, then +1+1+1 = 3
: trampoline indirect-dec ;
: indirect-dec ( n -- n )
  dup 0= if exit then
  1- trampoline 1+ ;
: main 3 indirect-dec ;
