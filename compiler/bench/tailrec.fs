\ expected: 100000000
\ Tail recursion, 100M iterations

: tailrec ( n acc -- acc ) recursive
  over 0= if nip exit then
  swap 1- swap 1+ recurse ;

: main
  100000000 0 tailrec . cr ;
