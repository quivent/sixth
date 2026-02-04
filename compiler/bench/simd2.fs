\ expected: 666566670000
\ Parallel multiply-add - vectorizable fused multiply-add

create a 80000 allot
create b 80000 allot

: init ( -- )
  10000 0 do
    i i 8 * a + !
    i 2 * i 8 * b + !
  loop ;

: dotprod ( -- n )
  0 10000 0 do
    i 8 * a + @ i 8 * b + @ * +
  loop ;

: main init dotprod . cr ;
