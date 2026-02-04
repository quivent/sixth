\ expected: 4999950000
\ BST insert 100K nodes, sum of depths

100000 constant N
create left N cells allot
create right N cells allot
create key N cells allot
variable root
variable depth-sum

: insert ( val -- )
  dup key root @ cells + !
  root @ 0= if
    0 depth-sum +!
    1 root !
    -1 left 0 cells + !
    -1 right 0 cells + !
    drop exit
  then
  0 root @  \ depth node
  begin
    over 1+ 2 pick key rot cells + @
    2 pick < if
      dup cells right + @
      dup 0< if
        drop
        root @ over cells right + !
        3 pick root @ cells key + !
        -1 root @ cells left + !
        -1 root @ cells right + !
        1 root +!
        depth-sum +!
        2drop exit
      then
      nip
    else
      dup cells left + @
      dup 0< if
        drop
        root @ over cells left + !
        3 pick root @ cells key + !
        -1 root @ cells left + !
        -1 root @ cells right + !
        1 root +!
        depth-sum +!
        2drop exit
      then
      nip
    then
  again ;

: lcg ( n -- n' ) 1103515245 * 12345 + 2147483647 and ;

: build-tree ( -- )
  1 root !
  0 depth-sum !
  50000 key 0 cells + !
  -1 left 0 cells + !
  -1 right 0 cells + !
  12345
  N 1 do
    lcg dup N mod insert
  loop
  drop ;

: main build-tree depth-sum @ . cr ;
