\ expect: 1 3 6 10 15
create arr 40 allot
: arr! ( val i -- ) 8 * arr + ! ;
: arr@ ( i -- val ) 8 * arr + @ ;
: prefix-sums ( n -- )
  1 do
    i 1- arr@ i arr@ + i arr!
  loop ;
: main
  1 0 arr!  2 1 arr!  3 2 arr!  4 3 arr!  5 4 arr!
  5 prefix-sums
  5 0 do i arr@ . loop cr ;
