\ Test 755: pure word consuming arg - must not be DCEd if result used
: add10 10 + ;
: main 5 add10 . cr ;
