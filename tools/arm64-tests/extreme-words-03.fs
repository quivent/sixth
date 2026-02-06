\ expect: 120
\ Extreme Test 03: Word calls itself through helper (indirect recursion)
\ Tests: recursion detection, stack frame management

: helper ( n -- n! ) fact ;
: fact ( n -- n! )
  dup 1 <= if drop 1 exit then
  dup 1 - helper * ;

: main 5 fact ;
