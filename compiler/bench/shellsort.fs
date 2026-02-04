\ expected: 49995000
\ Shell sort benchmark - sort 10000 elements, sum result

10000 constant SIZE
create arr SIZE cells allot

: arr@ ( i -- n ) cells arr + @ ;
: arr! ( n i -- ) cells arr + ! ;

: shell-sort ( -- )
  SIZE 2 /               \ gap
  begin dup 0> while
    SIZE over do         \ gap
      i arr@ i           \ gap key j
      begin
        dup 2 pick >= over 2 pick - arr@ 2 pick < and
      while
        dup 2 pick - arr@ over arr!
        over -
      repeat
      arr! drop
    loop
    2 /
  repeat drop ;

: init-arr ( -- )
  SIZE 0 do
    SIZE i - 1- i arr!
  loop ;

: sum-arr ( -- n )
  0 SIZE 0 do i arr@ + loop ;

: main
  init-arr
  shell-sort
  sum-arr . cr ;
