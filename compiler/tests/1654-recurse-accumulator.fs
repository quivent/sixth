\ expect: 15
: sum ( acc n -- acc ) recursive dup 0= if drop else dup rot + swap 1- recurse then ;
: main 0 5 sum . cr ;
