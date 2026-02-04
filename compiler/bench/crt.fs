\ expected: 1905079
\ Chinese Remainder Theorem - solve system of modular equations

: ext-gcd ( a b -- gcd x y )
  \ Extended Euclidean algorithm
  dup 0= if
    drop 1 0 exit
  then
  2dup mod swap recurse   \ r x1 y1  with a on stack
  rot                     \ x1 y1 r
  3 pick 3 pick /         \ x1 y1 a/b
  2 pick *                \ x1 y1 (a/b)*x1
  rot swap -              \ x1 y1' where y1'=y1-(a/b)*x1
  rot ;                   \ y1' x1 (swapped for result)

: mod-inv ( a m -- a^-1 )
  2dup ext-gcd drop       \ a m x
  nip swap mod
  dup 0< if over + then ;

: crt2 ( r1 m1 r2 m2 -- r m )
  \ Combine two equations: x = r1 (mod m1), x = r2 (mod m2)
  2over *                 \ r1 m1 r2 m2 m=m1*m2
  >r                      \ r1 m1 r2 m2  R: m
  over 3 pick mod-inv     \ r1 m1 r2 m2 m2^-1(mod m1)
  3 pick *                \ r1 m1 r2 m2 m2*m2^-1
  5 pick 5 pick -         \ r1 m1 r2 m2 m2*inv (r1-r2)
  * over +                \ r1 m1 r2 m2 x
  rot * +                 \ r1 m1 r  (r = r2 + m2*x)
  r@ mod
  dup 0< if r@ + then
  2swap 2drop r> ;

: main
  0
  100 1 do
    100 1 do
      i 17 j 23 crt2
      drop +
    loop
  loop
  . cr ;
