\ expect: 42
\ Chain of forward references: A->B->C, all forward
: a b 2 + ;
: b c 10 + ;
: c 30 ;
: main a ;
