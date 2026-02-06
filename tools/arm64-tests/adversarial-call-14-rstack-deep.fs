\ expect: 21
\ Return stack across 4 nested call levels
\ Each level saves its value, calls next, restores and adds
: d4 4 ;
: d3 3 >r d4 r> + ;
: d2 2 >r d3 r> + ;
: d1 1 >r d2 r> + ;
: d0 0 >r d1 r> + ;
: e4 5 ;
: e3 6 >r e4 r> + ;
: main d0 e3 + ;
