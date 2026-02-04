\ expected: 49995000
\ Radix sort (LSD) benchmark - sort 10000 elements, sum result

10000 constant SIZE
256 constant BASE
create arr SIZE cells allot
create tmp SIZE cells allot
create count BASE cells allot

: arr@ ( i -- n ) cells arr + @ ;
: arr! ( n i -- ) cells arr + ! ;
: tmp@ ( i -- n ) cells tmp + @ ;
: tmp! ( n i -- ) cells tmp + ! ;
: cnt@ ( i -- n ) cells count + @ ;
: cnt! ( n i -- ) cells count + ! ;

: clear-count ( -- )
  BASE 0 do 0 i cnt! loop ;

: get-digit ( n shift -- digit )
  rshift 255 and ;

: radix-pass ( shift -- )
  clear-count
  \ count occurrences
  SIZE 0 do
    i arr@ over get-digit
    dup cnt@ 1+ swap cnt!
  loop
  \ cumulative count
  1 BASE 1 do
    i 1- cnt@ i cnt@ + i cnt!
  loop
  \ build output (stable, descending order)
  SIZE 1- 0 swap do
    i arr@ dup 2 pick get-digit \ shift val digit
    dup cnt@ 1-                  \ shift val digit idx
    dup rot cnt!                 \ shift val idx
    tuck tmp! drop               \ shift
  -1 +loop
  drop
  \ copy back
  SIZE 0 do i tmp@ i arr! loop ;

: radix-sort ( -- )
  0 radix-pass
  8 radix-pass
  16 radix-pass
  24 radix-pass ;

: init-arr ( -- )
  SIZE 0 do
    SIZE i - 1- i arr!
  loop ;

: sum-arr ( -- n )
  0 SIZE 0 do i arr@ + loop ;

: main
  init-arr
  radix-sort
  sum-arr . cr ;
