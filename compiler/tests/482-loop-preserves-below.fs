\ expect: 42
\ Test 482: begin-until preserves stack below loop
\ Expected output: 42
: main 42 3 begin 1- dup 0= until drop . cr ;
