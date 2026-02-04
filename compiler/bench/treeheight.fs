\ expected: 16
\ Compute height of 100K node binary tree

100000 constant N
create left N cells allot
create right N cells allot
create height N cells allot

: init-tree ( -- )
  N 0 do
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

: max ( a b -- max ) 2dup < if swap then drop ;

: compute-heights ( -- )
  N 0 do 0 i cells height + ! loop
  N 1- 0 do
    N 1- i - dup cells left + @
    dup 0>= if
      cells height + @ 1+
    else drop 0 then
    over cells right + @
    dup 0>= if
      cells height + @ 1+
    else drop 0 then
    max
    N 1- i - cells height + !
  loop ;

: main init-tree compute-heights 0 cells height + @ . cr ;
