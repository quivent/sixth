\ Test 483: begin-while-repeat preserves stack below loop
\ Expected output: 42
: main 42 3 begin dup 0 > while 1- repeat drop . cr ;
