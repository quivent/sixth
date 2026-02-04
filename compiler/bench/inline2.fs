\ expected: 700000000
\ Chain of small functions - GCC inlines entire chain

: f1 ( n -- n ) 1+ ;
: f2 ( n -- n ) f1 1+ ;
: f3 ( n -- n ) f2 1+ ;
: f4 ( n -- n ) f3 1+ ;
: f5 ( n -- n ) f4 1+ ;
: f6 ( n -- n ) f5 1+ ;
: f7 ( n -- n ) f6 1+ ;

: main
  0 100000000 0 do f7 loop . cr ;
