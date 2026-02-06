\ expect: 36
\ Deep call chain: 8 levels, each adds its depth
\ Sum = 1+2+3+4+5+6+7+8 = 36
: level8 8 ;
: level7 level8 7 + ;
: level6 level7 6 + ;
: level5 level6 5 + ;
: level4 level5 4 + ;
: level3 level4 3 + ;
: level2 level3 2 + ;
: level1 level2 1 + ;
: main level1 ;
