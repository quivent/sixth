\ expected: 4999950000
\ Push then pop 100K elements, sum popped values

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

: sift-down ( i -- )
  begin
    dup 2* 1+ dup hsize @ < while
    dup 1+ hsize @ < if
      dup cells heap + @
      over 1+ cells heap + @
      < if 1+ then
    then
    2dup cells heap + @
    swap cells heap + @ < if
      2dup swap-heap
      nip
    else
      2drop exit
    then
  repeat
  2drop ;

: push ( val -- )
  hsize @ cells heap + !
  hsize @ sift-up
  1 hsize +! ;

: pop ( -- val )
  heap @
  hsize @ 1- hsize !
  hsize @ cells heap + @ heap !
  0 sift-down ;

: init ( -- )
  0 hsize !
  N 0 do i push loop ;

: pop-all ( -- sum )
  0 N 0 do pop + loop ;

: main init pop-all . cr ;
