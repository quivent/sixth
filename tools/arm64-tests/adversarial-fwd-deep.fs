\ expect: 7
\ Deep forward reference chain: 6 levels
: f1 f2 1 + ;
: f2 f3 1 + ;
: f3 f4 1 + ;
: f4 f5 1 + ;
: f5 f6 1 + ;
: f6 f7 1 + ;
: f7 1 ;
: main f1 ;
