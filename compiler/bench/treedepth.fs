\ expected: 17
\ Max depth recursively, simulate 100K node tree

: max ( a b -- max ) 2dup > if drop else nip then ;

: treedepth ( depth -- maxdepth ) recursive
  dup 0= if exit then
  1- dup recurse swap recurse max 1+ ;

: main
  1000 0 do 17 treedepth drop loop
  17 treedepth . cr ;
