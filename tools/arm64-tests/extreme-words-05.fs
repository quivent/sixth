\ expect: 77
\ Extreme Test 05: Forward references - call before definition
\ Tests: two-pass compilation, forward ref patching

: caller1 callee1 ;
: caller2 callee2 10 + ;
: caller3 callee3 callee3 + ;

: callee1 77 ;
: callee2 30 ;
: callee3 20 ;

: main caller1 caller2 drop caller3 drop ;
