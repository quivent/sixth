\ expected: 200000000
\ Peephole optimization - redundant operations

: redundant ( n -- n )
  1+ 1-   \ cancels out
  2 * 2 / \ cancels out (for positive)
  ;

: main
  0 100000000 0 do
    i redundant drop 2 +
  loop . cr ;
