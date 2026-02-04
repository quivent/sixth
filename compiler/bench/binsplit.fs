\ expected: 1048576
\ Binary split recursion, depth 20, 1000 times

: binsplit ( depth -- count ) recursive
  dup 0= if drop 1 exit then
  1- dup recurse swap recurse + ;

: main
  1000 0 do 20 binsplit drop loop
  20 binsplit . cr ;
