\ expect: 5 4 3 2 1
\ Reverse array in-place using variable for swap
create arr 40 allot
variable tmp
: a! ( val i -- ) cells arr + ! ;
: a@ ( i -- val ) cells arr + @ ;
: main
  1 0 a!  2 1 a!  3 2 a!  4 3 a!  5 4 a!
  \ Swap arr[0] and arr[4]
  0 a@ tmp !  4 a@ 0 a!  tmp @ 4 a!
  \ Swap arr[1] and arr[3]
  1 a@ tmp !  3 a@ 1 a!  tmp @ 3 a!
  5 0 do i a@ . loop cr ;
