\ expected: 2500000000000000
\ Branchless vs branch - GCC can use cmov

: absval ( n -- |n| ) dup 0< if negate then ;

: main
  0 100000000 0 do
    i 50000000 - absval +
  loop . cr ;
