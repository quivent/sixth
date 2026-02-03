\ expect: 0 0 0
\ Test 836: do loop using over to keep base value
: main ( -- ) 0 10 3 0 do over . loop 2drop cr ;
