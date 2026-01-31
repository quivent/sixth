\ expect: 25 7
: square ( n -- n*n ) dup * ;
: main 7 >r 5 square r> swap . . cr ;
