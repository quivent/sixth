\ Phase 12a: Constants used in expressions
\ expect: 100

constant SIZE 10
constant COUNT 10

: area ( -- n ) SIZE COUNT * ;
: main area ;
