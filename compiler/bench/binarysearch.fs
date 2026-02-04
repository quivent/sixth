\ expected: 499500
\ Binary search benchmark - search sorted array, 100 iterations

1000 constant SIZE
create arr SIZE cells allot

: arr@ ( i -- n ) cells arr + @ ;
: arr! ( n i -- ) cells arr + ! ;

: init-arr ( -- )
  SIZE 0 do i i arr! loop ;

: binary-search ( val -- idx|-1 )
  0 SIZE 1-
  begin 2dup <= while
    2dup + 2/
    dup arr@ 4 pick = if
      nip nip nip exit
    then
    dup arr@ 4 pick < if
      nip 1+
    else
      rot drop swap 1-
    then
  repeat
  2drop drop -1 ;

: bench-search ( -- sum )
  0
  SIZE 0 do
    i binary-search +
  loop ;

: main ( -- )
  init-arr
  0
  100 0 do
    drop bench-search
  loop
  . cr ;
