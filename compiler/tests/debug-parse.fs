\ debug-parse.fs - Test to debug runtime parsing
\ expect:

\ User word that prints directly
: hello ( -- ) ." HELLO" ;
: world ( -- ) ." WORLD" ;

: test1 ( -- )
  ." Before eval..." cr
  s" hello" evaluate
  ." After eval..." cr ;

: test2 ( -- )
  s" hello world" evaluate ;

: main
  ." === Test 1: single user word ===" cr
  test1
  ." === Test 2: two user words ===" cr
  test2
  cr
  ." Done." cr ;
