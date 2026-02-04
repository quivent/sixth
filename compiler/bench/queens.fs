\ expected: 92
\ 8-queens count solutions recursively

create board 8 allot
variable solutions

: safe? ( row col -- flag )
  0 begin
    dup 3 pick < while
    dup board + c@
    3 pick = if 2drop drop 0 exit then
    dup board + c@ 3 pick -
    2 pick 3 pick - abs = if 2drop drop 0 exit then
    1+
  repeat 2drop drop 1 ;

: queens ( col -- ) recursive
  dup 8 = if drop 1 solutions +! exit then
  8 0 do
    i over safe? if
      i over board + c!
      dup 1+ recurse
    then
  loop drop ;

: main
  0 solutions !
  0 queens
  solutions @ . cr ;
