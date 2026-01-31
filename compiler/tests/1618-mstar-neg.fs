\ expect: -1 -1000
\ m* ( n1 n2 -- lo hi )
\ -10 * 100 = -1000
\ In two's complement 64-bit: lo = -1000, hi = -1
variable lo
variable hi
: main
  -10 100 m*
  hi ! lo !
  hi @ . lo @ . cr ;
