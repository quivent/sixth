\ expect: 0
\ Test here alignment after c, (should not be aligned, just incremented)
\ Store 3 bytes, check if here advanced by 3
: main here 65 c, 66 c, 67 c, here swap - 3 - ;
