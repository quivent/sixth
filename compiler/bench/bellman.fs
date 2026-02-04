\ expected: 19900
\ Bellman-Ford shortest path, 200 nodes, sum distances

200 constant N
400 constant E
1000000 constant INF
create edges E 3 * cells allot
create dist N cells allot
variable nedges

: add-edge ( from to w -- )
  nedges @ 3 * cells edges +
  rot over !
  swap over cell+ !
  cell+ cell+ !
  1 nedges +! ;

: init-graph ( -- )
  0 nedges !
  N 1 do
    i 1- i 1 add-edge
  loop
  N 50 do
    i i 50 - 2 add-edge
  loop ;

: bellman ( start -- )
  N 0 do INF i cells dist + ! loop
  0 swap cells dist + !
  N 1 do
    nedges @ 0 do
      i 3 * cells edges + @
      dup cells dist + @ INF < if
        dup cells dist + @
        i 3 * cells edges + cell+ cell+ @
        +
        i 3 * cells edges + cell+ @
        dup cells dist + @
        2 pick < if
          drop cells dist + !
        else 2drop drop then
      else drop then
    loop
  loop ;

: sum-dist ( -- n )
  0 N 0 do
    i cells dist + @ dup INF < if + else drop then
  loop ;

: main init-graph 0 bellman sum-dist . cr ;
