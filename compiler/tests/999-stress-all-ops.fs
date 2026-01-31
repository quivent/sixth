\ Test 999: stress test combining stack ops, arithmetic, control flow, calls
: double 2* ;
: halve 2/ ;
: main 1 double double double double double halve halve halve 10 0 do dup . 1+ loop drop cr ;
