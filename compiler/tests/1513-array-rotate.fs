\ expect: 3 4 5 1 2
\ Rotate [1 2 3 4 5] left by 2 => [3 4 5 1 2]
create arr 40 allot
create tmp 40 allot
: arr@ ( i -- val ) 8 * arr + @ ;
: arr! ( val i -- ) 8 * arr + ! ;
: tmp@ ( i -- val ) 8 * tmp + @ ;
: tmp! ( val i -- ) 8 * tmp + ! ;
variable rn
variable rk
: rotate ( n k -- )
  rk !  rn !
  rn @ 0 do
    i rk @ + rn @ mod arr@ i tmp!
  loop
  rn @ 0 do
    i tmp@ i arr!
  loop ;
: main
  1 0 arr!  2 1 arr!  3 2 arr!  4 3 arr!  5 4 arr!
  5 2 rotate
  5 0 do i arr@ . loop cr ;
