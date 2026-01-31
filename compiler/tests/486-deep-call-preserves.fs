\ expect: 42
\ Test 486: deep function call preserves stack
\ Expected output: 42
: nop ;
: nop2 nop nop ;
: main 42 nop2 . cr ;
