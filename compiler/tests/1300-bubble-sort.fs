\ expect: 12 22 25 34 64
create arr 40 allot
: arr! ( val i -- ) 8 * arr + ! ;
: arr@ ( i -- val ) 8 * arr + @ ;
variable tmp
variable idx
: swap-at ( -- )
  idx @ arr@ tmp !
  idx @ 1+ arr@ idx @ arr!
  tmp @ idx @ 1+ arr! ;
: bubble1 ( -- )
  4 0 do
    i arr@ i 1+ arr@ > if i idx ! swap-at then
  loop ;
: main
  64 0 arr!  34 1 arr!  25 2 arr!  12 3 arr!  22 4 arr!
  bubble1 bubble1 bubble1 bubble1
  5 0 do i arr@ . loop cr ;
