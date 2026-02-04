\ expected: 142100000000
\ Dot product of 1000-element vectors, 100K times
\ Checksum: sum of all dot products

create vecA 1000 cells allot
create vecB 1000 cells allot

: init-vecs ( iter -- )
  1000 0 do
    dup i + 100 mod vecA i cells + !
    dup i + 50 mod vecB i cells + !
  loop drop ;

: dot-product ( -- result )
  0
  1000 0 do
    vecA i cells + @
    vecB i cells + @
    * +
  loop ;

: bench-dotprod ( -- sum )
  0
  100000 0 do
    i init-vecs
    dot-product +
  loop ;

: main bench-dotprod . cr ;
