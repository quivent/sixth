\ expect: 0 1 2 30 20 10
\ Test 847: three values below do loop preserved
: main 10 20 30 3 0 do i . loop . . . cr ;
