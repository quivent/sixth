\ expect: 45
\ Many small words calling each other
\ Simulates fine-grained code: add1 through add9
: add1 1 + ;
: add2 add1 add1 ;
: add3 add2 add1 ;
: add4 add3 add1 ;
: add5 add4 add1 ;
: add6 add5 add1 ;
: add7 add6 add1 ;
: add8 add7 add1 ;
: add9 add8 add1 ;
: all 0 add1 add2 add3 add4 add5 add6 add7 add8 add9 ;
: main all ;
