\ expected: 6552127682118890048
\ Pascal triangle row 500 (C(500,250) mod 2^63)
\ Uses memoization for practical performance

501 constant PSIZE
create pmemo PSIZE PSIZE * cells allot
create pcomp PSIZE PSIZE * allot

: pidx ( n k -- addr ) PSIZE * + ;
: pmget ( n k -- val ) pidx cells pmemo + @ ;
: pmset ( val n k -- ) pidx cells pmemo + ! ;
: pcget ( n k -- flag ) pidx pcomp + c@ ;
: pcset ( n k -- ) pidx pcomp + 1 swap c! ;

: pascal ( n k -- c ) recursive
  over over = if 2drop 1 exit then
  dup 0= if 2drop 1 exit then
  2dup pcget if pmget exit then
  over 1- over 1- recurse
  2 pick 1- 2 pick recurse +
  dup 3 pick 3 pick pmset
  -rot pcset ;

: main
  PSIZE PSIZE * 0 do 0 i pcomp + c! loop
  500 250 pascal . cr ;
