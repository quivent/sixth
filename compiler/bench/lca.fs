\ expected: 1023332
\ LCA queries on 100K node tree, 100K queries, sum of LCA values

100000 constant N
create parent N cells allot
create depth N cells allot

: init-tree ( -- )
  0 0 cells parent + !
  0 0 cells depth + !
  N 1 do
    i 1- 2/ i cells parent + !
    i 1- 2/ cells depth + @ 1+ i cells depth + !
  loop ;

: lca ( a b -- lca )
  begin
    2dup depth swap cells + @
    over depth swap cells + @ > while
    parent swap cells + @
  repeat
  begin
    over depth swap cells + @
    2 pick depth swap cells + @ > while
    swap parent swap cells + @ swap
  repeat
  begin 2dup <> while
    parent swap cells + @
    swap parent swap cells + @ swap
  repeat
  drop ;

: lcg ( n -- n' ) 1103515245 * 12345 + 2147483647 and ;

: run-queries ( -- sum )
  0
  12345
  N 0 do
    lcg dup N mod
    over lcg N mod lca +
    swap lcg nip
  loop
  drop ;

: main init-tree run-queries . cr ;
