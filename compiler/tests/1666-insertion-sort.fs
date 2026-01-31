\ expect: 1 3 5 8 9
\ Insertion sort 5 elements
create arr 40 allot
variable tmp
: a! ( val i -- ) cells arr + ! ;
: a@ ( i -- val ) cells arr + @ ;
: bpass ( -- )
  4 0 do
    i a@ i 1+ a@ > if
      i a@ tmp !
      i 1+ a@ i a!
      tmp @ i 1+ a!
    then
  loop ;
: main
  5 0 a!  3 1 a!  8 2 a!  1 3 a!  9 4 a!
  bpass bpass bpass bpass
  5 0 do i a@ . loop cr ;
