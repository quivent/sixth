\ expect: 0
\ Test: Deep call chain - 12 levels of function calls
\ Each level adds 1 to the accumulator

: level12 ( n -- n ) 1 + ;
: level11 ( n -- n ) 1 + level12 ;
: level10 ( n -- n ) 1 + level11 ;
: level9  ( n -- n ) 1 + level10 ;
: level8  ( n -- n ) 1 + level9 ;
: level7  ( n -- n ) 1 + level8 ;
: level6  ( n -- n ) 1 + level7 ;
: level5  ( n -- n ) 1 + level6 ;
: level4  ( n -- n ) 1 + level5 ;
: level3  ( n -- n ) 1 + level4 ;
: level2  ( n -- n ) 1 + level3 ;
: level1  ( n -- n ) 1 + level2 ;
: start   ( -- n )   0 level1 ;

: main start 12 - ;
