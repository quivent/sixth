\ expected: 1000
\ Maze solver using flood fill

16 constant SIZE
create maze SIZE SIZE * allot
create visited SIZE SIZE * allot

: m@ ( x y -- val ) SIZE * + maze + c@ ;
: m! ( val x y -- ) SIZE * + maze + c! ;
: v@ ( x y -- val ) SIZE * + visited + c@ ;
: v! ( val x y -- ) SIZE * + visited + c! ;

: in-bounds? ( x y -- flag )
  dup 0>= swap SIZE < and
  swap dup 0>= swap SIZE < and and ;

: init-maze ( -- )
  \ Create simple maze with path from (0,0) to (15,15)
  SIZE SIZE * 0 do 0 maze i + c! loop
  SIZE SIZE * 0 do 0 visited i + c! loop
  \ Clear path along edges and diagonal
  SIZE 0 do 1 i 0 m! loop        \ top row
  SIZE 0 do 1 SIZE 1- i m! loop  \ bottom row
  SIZE 0 do 1 0 i m! loop        \ left column
  SIZE 0 do 1 i SIZE 1- m! loop  \ right column
  ;

variable found

: solve ( x y goalx goaly -- found? )
  2over 2over = swap 3 pick = and if
    2drop 2drop 1 exit
  then
  2over in-bounds? 0= if 2drop 2drop 0 exit then
  2over m@ 0= if 2drop 2drop 0 exit then
  2over v@ if 2drop 2drop 0 exit then
  2over 1 rot rot v!
  \ Try all 4 directions
  2over 1+ 2over recurse if 2drop 2drop 1 exit then
  2over 1- 2over recurse if 2drop 2drop 1 exit then
  2over swap 1+ swap 2over recurse if 2drop 2drop 1 exit then
  2over swap 1- swap 2over recurse if 2drop 2drop 1 exit then
  2drop 2drop 0 ;

: run-maze ( -- found? )
  SIZE SIZE * 0 do 0 visited i + c! loop
  0 0 SIZE 1- SIZE 1- solve ;

: main
  0
  1000 0 do
    init-maze
    run-maze if 1+ then
  loop
  . cr ;
