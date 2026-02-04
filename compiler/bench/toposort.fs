\ expected: 332833500
\ Topological sort on DAG of 1000 nodes, sum of sorted positions * node

1000 constant N
create adj N 5 * cells allot
create deg N cells allot
create indegree N cells allot
create result N cells allot
create queue N cells allot

: edge ( from to -- )
  over cells deg + @ 5 * rot + cells adj + !
  1 over cells indegree + +!
  swap cells deg + dup @ 1+ swap ! ;

: init-graph ( -- )
  N 0 do
    0 i cells deg + !
    0 i cells indegree + !
  loop
  N 1 do
    i 1- i edge
  loop ;

: toposort ( -- )
  0 0  \ qhead qtail
  N 0 do
    i cells indegree + @ 0= if
      i over cells queue + !
      swap 1+ swap
    then
  loop
  0  \ ridx
  begin 2 pick 2 pick < while
    3 pick cells queue + @
    4 roll 1+ 4 -roll
    dup 2 pick cells result + !
    rot 1+ -rot
    dup cells deg + @ 0 ?do
      over 5 * i + cells adj + @
      dup cells indegree + @ 1-
      dup 2 pick cells indegree + !
      0= if
        3 pick cells queue + !
        swap 1+ swap
      else drop then
    loop
    drop
  repeat
  drop 2drop ;

: checksum ( -- n )
  0 N 0 do
    i cells result + @ i * +
  loop ;

: main init-graph toposort checksum . cr ;
