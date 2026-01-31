\ expect: 8 3
\ Test 752: helper using 2dup
: sq-both 2dup * rot rot + ;
: main 3 4 sq-both . . cr ;
