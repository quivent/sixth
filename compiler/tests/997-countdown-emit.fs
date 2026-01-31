\ Test 997: countdown from 5 to 1 printing digits as ASCII
\ '5'=53, '4'=52, '3'=51, '2'=50, '1'=49
: digit 48 + emit ;
: main 5 begin dup while dup digit 1- repeat drop cr ;
