\ expected: 724
\ 10-queens count solutions recursively

create board10 10 allot
variable solutions10

: safe10? ( row col -- flag )
  0 begin
    dup 3 pick < while
    dup board10 + c@
    3 pick = if 2drop drop 0 exit then
    dup board10 + c@ 3 pick -
    2 pick 3 pick - abs = if 2drop drop 0 exit then
    1+
  repeat 2drop drop 1 ;

: queens10 ( col -- ) recursive
  dup 10 = if drop 1 solutions10 +! exit then
  10 0 do
    i over safe10? if
      i over board10 + c!
      dup 1+ recurse
    then
  loop drop ;

: main
  0 solutions10 !
  0 queens10
  solutions10 @ . cr ;
