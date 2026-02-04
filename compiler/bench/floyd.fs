\ expected: 90500
\ Floyd-Warshall all-pairs shortest path, 100 nodes

100 constant N
1000000 constant INF
create dist N N * cells allot

: idx ( i j -- addr ) swap N * + cells dist + ;

: init ( -- )
  N 0 do N 0 do
    i j = if 0 else INF then i j idx !
  loop loop
  N 1 do
    1 i i 1- idx !
    1 i 1- i idx !
  loop
  N 10 do
    2 i i 10 - idx !
    2 i 10 - i idx !
  loop ;

: min ( a b -- min ) 2dup > if swap then drop ;

: floyd ( -- )
  N 0 do
    N 0 do
      N 0 do
        j k idx @
        j i idx @ i k idx @ + min
        j k idx !
      loop
    loop
  loop ;

: sum-dist ( -- n )
  0 N 0 do N 0 do
    j i idx @ dup INF < if + else drop then
  loop loop ;

: main init floyd sum-dist . cr ;
