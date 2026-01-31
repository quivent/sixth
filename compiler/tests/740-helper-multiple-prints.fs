\ expect: 10 20
\ Test 740: helper with multiple prints not DCEd
: show-both swap . . ;
: main 10 20 show-both cr ;
