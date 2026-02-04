\ expected: 4458
\ Kruskal MST, 500 edges, sum of MST edge weights

500 constant NODES
500 constant EDGES
create parent NODES cells allot
create rank NODES cells allot
create edges EDGES 3 * cells allot  \ from, to, weight

: find ( x -- root )
  begin
    dup cells parent + @
    2dup <> while
    nip
  repeat
  drop ;

: union ( x y -- united? )
  find swap find
  2dup = if 2drop 0 exit then
  over cells rank + @
  over cells rank + @
  2dup < if
    2drop swap cells parent + !
  else
    2dup > if
      2drop cells parent + !
    else
      drop cells parent + !
      dup cells rank + @ 1+ swap cells rank + !
    then
  then
  1 ;

: init-uf ( -- )
  NODES 0 do
    i i cells parent + !
    0 i cells rank + !
  loop ;

: edge! ( from to w idx -- )
  3 * cells edges +
  dup rot swap 2 cells + !
  swap over 1 cells + !
  swap swap ! ;

: edge@ ( idx -- from to w )
  3 * cells edges +
  dup @
  over 1 cells + @
  rot 2 cells + @ ;

: init-edges ( -- )
  EDGES 0 do
    i NODES 1- mod
    i NODES 1- mod 1+
    i 17 mod 1+
    i edge!
  loop ;

: sort-edges ( -- )  \ bubble sort by weight
  EDGES 1 do
    i 0 do
      i edge@ drop nip
      i 1+ edge@ drop nip
      > if
        i edge@
        i 1+ edge@
        i edge!
        i 1+ edge!
      then
    loop
  loop ;

: kruskal ( -- mst-weight )
  init-uf
  0 0  \ count weight
  EDGES 0 do
    over NODES 1- >= if leave then
    i edge@
    rot drop
    union if
      swap 1+ swap
      i edge@ nip nip +
    then
  loop
  nip ;

: main init-edges sort-edges kruskal . cr ;
