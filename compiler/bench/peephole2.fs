\ expected: 500000000
\ Instruction combining - GCC combines ops

: combine ( n -- n )
  3 +
  2 +   \ GCC combines to +5
  ;

: main
  0 100000000 0 do
    i combine drop 5 +
  loop . cr ;
