\ expected: 100000000
\ Unreachable branch elimination - always false condition

: process ( n -- n )
  dup 0 < if
    1000000 *   \ never reached with positive input
  then ;

: main
  0 100000000 0 do
    i process drop 1+
  loop . cr ;
