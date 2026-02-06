\ expect: 42
\ Test: EXIT inside IF statement
\ This is a fundamental control flow test
\ BUG DOCUMENTED: As of this writing, EXIT inside IF corrupts return

: main
  1 if 42 exit then
  99 ;
