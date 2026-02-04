\ expected: 352
\ Push 100K elements to heap, return max

100000 constant N
create heap N cells allot
variable hsize

: swap-heap ( i j -- )
  cells heap + swap cells heap +
  over @ over @
  rot ! swap ! ;

: sift-up ( i -- )
  begin
    dup 0> while
    dup 1- 2/
    over cells heap + @
    over cells heap + @
    > if
      2dup swap-heap
      nip
    else
      2drop exit
    then
  repeat
  drop ;

: push ( val -- )
  hsize @ cells heap + !
  hsize @ sift-up
  1 hsize +! ;

: init ( -- )
  0 hsize !
  N 0 do
    i 17 mod i 23 mod * N mod push
  loop ;

: main init heap @ . cr ;
