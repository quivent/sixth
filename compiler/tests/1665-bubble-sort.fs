\ expect: 3 7 12 25 64
\ Bubble sort 5 elements using variable for temp
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
  64 0 a!  25 1 a!  12 2 a!  3 3 a!  7 4 a!
  bpass bpass bpass bpass
  5 0 do i a@ . loop cr ;
