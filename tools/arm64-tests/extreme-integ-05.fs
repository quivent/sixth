\ expect: 0
\ Test: Return stack across multiple call levels with loops
\ r> and >r survival through deep call chains

variable depth

: inner ( n -- n' )
  >r
  r@ 2 *
  r> + ;

: middle ( n -- n' )
  dup >r
  inner
  r> + ;

: outer1 ( n -- n' )
  dup >r
  middle
  r> + ;

: apply3 ( n -- n' )
  outer1 outer1 outer1 ;

: main
  1 apply3
  \ inner(n) = n*2 + n = 3n
  \ middle(n) = inner(n) + n = 3n + n = 4n
  \ outer1(n) = middle(n) + n = 4n + n = 5n
  \ outer1(1)=5, outer1(5)=25, outer1(25)=125
  125 = if 0 else 1 then ;
