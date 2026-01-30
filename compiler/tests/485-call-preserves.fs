\ Test 485: function call preserves stack
\ Expected output: 42
: nop ;
: main 42 nop . cr ;
