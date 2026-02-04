\ expected: 1000
\ DFS on 1000-node graph, count visited nodes

1000 constant N
create adj N 10 * cells allot
create deg N cells allot
create visited N allot
create stack N cells allot
variable count

: edge ( from to -- )
  over cells deg + @ 10 * rot + cells adj + !
  swap cells deg + dup @ 1+ swap ! ;

: init-graph ( -- )
  N 0 do 0 i cells deg + ! loop
  N 1 do
    i i 1- edge
    i 7 mod 0= if i i 7 / edge then
  loop ;

: dfs ( start -- n )
  N 0 do 0 visited i + c! loop
  0 count !
  0  \ sp = stack pointer
  swap over cells stack + !  \ push start
  1+
  begin dup 0> while
    1- dup cells stack + @  \ pop
    dup visited + c@ 0= if
      1 over visited + c!
      1 count +!
      dup cells deg + @
      swap cells deg + @ 0 ?do
        over 10 * i + cells adj + @
        dup visited + c@ 0= if
          2 pick cells stack + !
          swap 1+ swap
        else drop then
      loop
    else drop then
  repeat
  drop count @ ;

: main init-graph 0 dfs . cr ;
