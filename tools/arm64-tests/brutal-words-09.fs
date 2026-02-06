\ expect: 0
\ Test: Constants across word boundaries
\ Multiple words using shared constants

constant BASE 10
constant CENTURY 100
constant KILO 1000

: in-base ( n -- n*base ) BASE * ;
: in-centuries ( n -- n*100 ) CENTURY * ;
: in-kilos ( n -- n*1000 ) KILO * ;

: check ( -- n )
  5 in-base 50 <> if 1 exit then
  3 in-centuries 300 <> if 2 exit then
  2 in-kilos 2000 <> if 3 exit then
  0 ;

: main check ;
