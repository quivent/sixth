\ expected: 336000
\ Knight's tour - count valid knight moves

8 constant SIZE
create board SIZE SIZE * allot

: b@ ( x y -- val ) SIZE * + board + c@ ;
: b! ( val x y -- ) SIZE * + board + c! ;

: in-bounds? ( x y -- flag )
  dup 0>= swap SIZE < and
  swap dup 0>= swap SIZE < and and ;

\ Knight move offsets
create dx 2 , 1 , -1 , -2 , -2 , -1 , 1 , 2 ,
create dy 1 , 2 , 2 , 1 , -1 , -2 , -2 , -1 ,

: count-valid-moves ( x y -- count )
  0
  8 0 do
    2 pick dx i cells + @ +
    2 pick dy i cells + @ +
    2dup in-bounds? if
      2dup b@ 0= if rot 1+ rot rot then
    then
    2drop
  loop
  nip nip ;

: init-board ( -- )
  SIZE SIZE * 0 do 0 board i + c! loop ;

: main
  0
  1000 0 do
    init-board
    SIZE 0 do SIZE 0 do
      j i count-valid-moves +
    loop loop
  loop
  . cr ;
