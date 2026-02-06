\ expect: 0
\ Test: Few large words - complex logic in single words
\ Tests code generation for larger word bodies

: square ( n -- n^2 ) dup * ;

: sum-of-squares ( a b -- a^2+b^2 )
  square swap square + ;

: poly ( a b c -- result )
  >r + r> * ;

: quad ( a b c d -- result )
  + >r + r> * ;

: check ( -- n )
  5 square 25 <> if 1 exit then
  3 4 sum-of-squares 25 <> if 2 exit then
  3 4 5 poly 35 <> if 3 exit then
  2 3 4 5 quad 45 <> if 4 exit then
  0 ;

: main check ;
