\ expect: 55
\ Extreme Test 02: Mutual recursion with base case
\ Tests: forward reference handling, mutual call patterns

: even? ( n -- flag ) dup 0= if drop 1 exit then 1 - odd? ;
: odd? ( n -- flag ) dup 0= if drop 0 exit then 1 - even? ;

: main
  10 even? 1 = if 55 else 0 then ;
