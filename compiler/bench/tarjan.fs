\ expected: 500
\ Tarjan SCC, 500 nodes forming 500 SCCs (each node is its own SCC)

500 constant N
create adj N 4 * cells allot
create deg N cells allot
create disc N cells allot
create low N cells allot
create onstack N allot
create stack N cells allot
variable sp
variable timer
variable scc-count

: edge ( from to -- )
  over cells deg + @ 4 * rot + cells adj + !
  swap cells deg + dup @ 1+ swap ! ;

: init-graph ( -- )
  N 0 do
    0 i cells deg + !
    -1 i cells disc + !
    0 i onstack + c!
  loop
  N 1 do
    i 1- i edge
  loop ;

: min ( a b -- min ) 2dup > if swap then drop ;

: tarjan-visit ( u -- )
  timer @ dup rot cells disc + !
  dup 1 pick cells low + !
  timer @ 1+ timer !
  1 over onstack + c!
  dup sp @ cells stack + ! sp @ 1+ sp !
  dup cells deg + @ 0 ?do
    dup 4 * i + cells adj + @
    dup cells disc + @ 0< if
      recurse
      2dup cells low + @ swap cells low + @ min
      over cells low + !
    else
      dup onstack + c@ if
        2dup cells disc + @ swap cells low + @ min
        over cells low + !
      then
    then
    drop
  loop
  dup cells disc + @ over cells low + @ = if
    1 scc-count +!
    begin
      sp @ 1- dup sp !
      cells stack + @
      0 over onstack + c!
      over =
    until
  then
  drop ;

: tarjan ( -- )
  0 timer ! 0 sp ! 0 scc-count !
  N 0 do
    i cells disc + @ 0< if i tarjan-visit then
  loop ;

: main init-graph tarjan scc-count @ . cr ;
