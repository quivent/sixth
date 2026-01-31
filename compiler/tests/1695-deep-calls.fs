\ expect: 170
\ Deeply nested function calls, 7 levels deep
: f1 ( n -- n ) 1+ ;
: f2 ( n -- n ) f1 f1 ;
: f3 ( n -- n ) f2 f2 ;
: f4 ( n -- n ) f3 f3 ;
: f5 ( n -- n ) f4 f4 ;
: f6 ( n -- n ) f5 f5 ;
: f7 ( n -- n ) f6 f6 ;
: main 42 f7 f7 . cr ;
