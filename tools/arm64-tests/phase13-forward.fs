\ expect: 42
\ Forward reference: main calls helper before helper is defined
: main helper 2 * ;
: helper 21 ;
