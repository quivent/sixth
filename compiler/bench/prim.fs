\ expected: 499
\ Prim MST, 500 nodes connected in line, MST weight = 499

500 constant N
1000000 constant INF
create adj N 4 * cells allot
create weight N 4 * cells allot
create deg N cells allot
create key N cells allot
create inMST N allot
variable mst-weight

: edge ( from to w -- )
  2 pick cells deg + @ 4 *
  3 pick + cells weight + !
  over cells deg + @ 4 *
  2 pick + cells adj + !
  swap cells deg + dup @ 1+ swap ! ;

: init-graph ( -- )
  N 0 do 0 i cells deg + ! loop
  N 1 do
    i i 1- 1 edge
    i 1- i 1 edge
  loop ;

: find-min ( -- node|-1 )
  -1 INF
  N 0 do
    i inMST + c@ 0= if
      i cells key + @ over < if
        2drop i i cells key + @
      then
    then
  loop
  drop ;

: prim ( -- )
  N 0 do
    INF i cells key + !
    0 i inMST + c!
  loop
  0 0 cells key + !
  0 mst-weight !
  N 0 do
    find-min dup 0< if drop unloop exit then
    dup cells key + @ mst-weight +!
    1 over inMST + c!
    dup cells deg + @ 0 ?do
      dup 4 * i + cells adj + @
      dup inMST + c@ 0= if
        2dup cells key + @
        2 pick 4 * i + cells weight + @
        over > if
          nip swap cells key + !
        else 2drop drop then
      else drop then
    loop
    drop
  loop ;

: main init-graph prim mst-weight @ . cr ;
