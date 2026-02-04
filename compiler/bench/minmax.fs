\ expected: 65536
\ Find min and max benchmark - 1M elements

1000000 constant SIZE
create arr SIZE cells allot

: arr@ ( i -- n ) cells arr + @ ;
: arr! ( n i -- ) cells arr + ! ;

: find-minmax ( -- min max )
  0 arr@ 0 arr@           \ min max
  SIZE 1 do
    i arr@
    dup 2 pick < if       \ new min
      rot drop swap
    else dup 2 pick > if  \ new max
      nip
    else
      drop
    then then
  loop ;

: init-arr ( -- )
  SIZE 0 do
    i 31337 * 65537 mod i arr!
  loop ;

: main
  init-arr
  find-minmax + . cr ;
