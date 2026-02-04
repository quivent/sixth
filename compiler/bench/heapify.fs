\ expected: 352
\ Build max-heap from 100K elements, return max

100000 constant N
create heap N cells allot

: swap-heap ( i j -- )
  cells heap + swap cells heap +
  over @ over @
  rot ! swap ! ;

: sift-down ( i -- )
  begin
    dup 2* 1+ dup N < while
    dup 1+ N < if
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

: heapify ( -- )
  N 2/ 1- 0 do
    N 2/ 1- i - sift-down
  loop ;

: init-heap ( -- )
  N 0 do
    i 17 mod i 23 mod * N mod i cells heap + !
  loop ;

: main init-heap heapify heap @ . cr ;
