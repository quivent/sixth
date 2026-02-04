\ expected: 5
\ Game of Life - run simulation steps

32 constant SIZE
create grid SIZE SIZE * allot
create next-grid SIZE SIZE * allot

: idx ( x y -- addr ) SIZE * + ;
: g@ ( x y -- val ) idx grid + c@ ;
: g! ( val x y -- ) idx grid + c! ;
: n@ ( x y -- val ) idx next-grid + c@ ;
: n! ( val x y -- ) idx next-grid + c! ;

: wrap ( n -- n' ) SIZE + SIZE mod ;

: count-neighbors ( x y -- count )
  0
  3 0 do 3 0 do
    i 1 = j 1 = and if else
      4 pick i + 1- wrap
      4 pick j + 1- wrap
      g@ +
    then
  loop loop
  nip nip ;

: step-cell ( x y -- )
  2dup count-neighbors   \ x y neighbors
  2 pick 2 pick g@       \ x y neighbors alive?
  if
    dup 2 < if drop 0 else 3 > if 0 else 1 then then
  else
    3 = if 1 else 0 then
  then
  rot rot n! ;

: step ( -- )
  SIZE 0 do SIZE 0 do
    j i step-cell
  loop loop
  SIZE SIZE * 0 do
    next-grid i + c@ grid i + c!
  loop ;

: count-alive ( -- n )
  0 SIZE SIZE * 0 do grid i + c@ + loop ;

: init-glider ( -- )
  SIZE SIZE * 0 do 0 grid i + c! loop
  1 1 2 g!  1 2 3 g!  1 3 1 g!  1 3 2 g!  1 3 3 g! ;

: main
  init-glider
  1000 0 do step loop
  count-alive . cr ;
