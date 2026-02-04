\ expected: 67063
\ Dijkstra shortest path, 500 nodes, sum of distances from node 0

500 constant N
2147483647 constant INF
create dist N cells allot
create done N allot
create adj N 10 * cells allot    \ neighbor node
create weight N 10 * cells allot \ edge weight
create deg N cells allot

: edge ( from to w -- )
  2 pick cells deg + @ 10 *
  3 pick + cells weight + !
  over cells deg + @ 10 *
  2 pick + cells adj + !
  swap cells deg + dup @ 1+ swap ! ;

: init-graph ( -- )
  N 0 do 0 i cells deg + ! loop
  N 1 do
    i i 1- i 17 mod 1+ edge
  loop
  N 50 do
    i i 50 - i 23 mod 1+ edge
  loop ;

: find-min ( -- node|-1 )
  -1 INF
  N 0 do
    i done + c@ 0= if
      i cells dist + @ over < if
        2drop i i cells dist + @
      then
    then
  loop
  drop ;

: dijkstra ( start -- )
  N 0 do INF i cells dist + ! 0 i done + c! loop
  0 swap cells dist + !
  N 0 do
    find-min dup 0< if drop unloop exit then
    1 over done + c!
    dup cells dist + @
    over cells deg + @ 0 ?do
      2dup 10 * i + cells adj + @      \ neighbor
      3 pick 10 * i + cells weight + @ \ weight
      rot + -rot                        \ alt = dist[u] + w
      dup cells dist + @
      2 pick over > if
        drop 2 pick swap cells dist + !
      else 2drop then
      swap
    loop
    2drop
  loop ;

: sum-dist ( -- n )
  0 N 0 do i cells dist + @ + loop ;

: main init-graph 0 dijkstra sum-dist . cr ;
