\ expect: 0 100000
\ m* ( n1 n2 -- d ) where d is ( lo hi ) on stack
\ 1000 * 100 = 100000, fits in lo, hi=0
\ . . prints hi then lo
variable lo
variable hi
: main
  1000 100 m*
  hi ! lo !
  hi @ . lo @ . cr ;
