\ expected: 12497500
\ Insertion sort benchmark - sort 5000 elements, sum result

5000 constant SIZE
create arr SIZE cells allot

: arr@ ( i -- n ) cells arr + @ ;
: arr! ( n i -- ) cells arr + ! ;

: isort ( -- )
  SIZE 1 do
    i arr@ i               \ key j
    begin
      dup 0> over 1- arr@ 2 pick < and
    while
      dup 1- arr@ over arr!
      1-
    repeat
    arr!
  loop ;

: init-arr ( -- )
  SIZE 0 do
    SIZE i - 1- i arr!
  loop ;

: sum-arr ( -- n )
  0 SIZE 0 do i arr@ + loop ;

: main
  init-arr
  isort
  sum-arr . cr ;
