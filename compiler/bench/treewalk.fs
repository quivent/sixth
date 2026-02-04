\ expected: 4999950000
\ Inorder traversal of 100K nodes, sum values

100000 constant N
create left N cells allot
create right N cells allot
create val N cells allot
variable sum
variable sp
create stack N cells allot

: build-balanced ( lo hi -- )
  2dup > if 2drop exit then
  2dup + 2/ dup val rot cells + !
  2dup + 2/ dup left rot cells + !
  2dup + 2/ dup right rot cells + !
  over 1- over over left swap cells + @ swap
  2dup > if
    -1 2 pick left swap cells + !
    2drop
  else
    cells val + !
    recurse
  then
  over 1+ swap over right swap cells + @ swap
  2dup > if
    -1 2 pick right swap cells + !
    2drop
  else
    cells val + !
    recurse
  then ;

: init-tree ( -- )
  N 0 do
    i i cells val + !
    -1 i cells left + !
    -1 i cells right + !
  loop
  N 1 do
    i 1- 2/ i cells left + @ 0< if
      i i 1- 2/ cells left + !
    else
      i i 1- 2/ cells right + !
    then
  loop ;

: inorder ( -- )
  0 sum !
  0 sp !
  0  \ current node
  begin
    dup 0>= while
    dup sp @ cells stack + ! sp @ 1+ sp !
    cells left + @
  repeat
  drop
  begin
    sp @ 0> while
    sp @ 1- dup sp ! cells stack + @
    dup cells val + @ sum +!
    cells right + @
    begin
      dup 0>= while
      dup sp @ cells stack + ! sp @ 1+ sp !
      cells left + @
    repeat
    drop
  repeat ;

: main init-tree inorder sum @ . cr ;
