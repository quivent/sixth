\ expect: 7 12
\ Test 752: helper using 2dup
: sq-both ( a b -- sum product ) 2dup * rot rot + ;
: main 3 4 sq-both . . cr ;
