\ expected: 1000
\ BFS on 1000-node graph, count visited nodes

1000 constant N
create adj N 10 * cells allot  \ adjacency list storage
create deg N cells allot       \ degree of each node
create visited N allot
create queue N cells allot

: edge ( from to -- )
  over cells deg + @ 10 * rot + cells adj + !
  swap cells deg + dup @ 1+ swap ! ;

: init-graph ( -- )
  N 0 do 0 i cells deg + ! loop
  N 1 do
    i i 1- edge  \ linear chain
    i 7 mod 0= if i i 7 / edge then  \ some cross edges
  loop ;

: bfs ( start -- count )
  N 0 do 0 visited i + c! loop
  0 0  \ qhead qtail
  rot dup visited + 1 swap c!
  2 pick cells queue + !  \ queue[qtail] = start
  swap 1+ swap            \ qtail++
  1                       \ count = 1
  begin 2 pick 2 pick < while  \ qhead < qtail
    3 pick cells queue + @  \ node = queue[qhead]
    4 roll 1+ 4 -roll       \ qhead++
    dup cells deg + @       \ get degree
    over cells deg + @ 0 ?do
      over 10 * i + cells adj + @  \ neighbor
      dup visited + c@ 0= if
        1 over visited + c!
        5 roll dup 1+ 6 -roll  \ qtail++
        cells 4 pick 1- cells queue + !
        rot 1+ -rot  \ count++
      else drop then
    loop
    drop
  repeat
  nip nip nip ;

: main init-graph 0 bfs . cr ;
