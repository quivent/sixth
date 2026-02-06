\ expect: 30
\ Stack values passing through a call chain
\ Pass 3 values through 5 levels
: level5 ( a b c -- sum ) + + ;
: level4 ( a b c -- sum ) level5 ;
: level3 ( a b c -- sum ) level4 ;
: level2 ( a b c -- sum ) level3 ;
: level1 ( a b c -- sum ) level2 ;
: main 5 10 15 level1 ;
