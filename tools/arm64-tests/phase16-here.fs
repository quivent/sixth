\ expect: 8
\ Test here: allocate 8 bytes, here should advance by 8
: main here 8 allot here swap - ;
