\ expect: 28
\ Deep call chain: 7 levels, returns sum 1+2+...+7 = 28
: d7 7 ;
: d6 d7 6 + ;
: d5 d6 5 + ;
: d4 d5 4 + ;
: d3 d4 3 + ;
: d2 d3 2 + ;
: d1 d2 1 + ;
: main d1 ;
