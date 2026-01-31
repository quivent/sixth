\ expect: 243
\ Test 995: 3^5 = 243
: power ( b e -- n ) 1 swap 0 do over * loop nip ;
: main 3 5 power . cr ;
