\ expect: 3
\ Count occurrences of value 7 in array
create arr 64 allot
: a! ( val i -- ) cells arr + ! ;
: a@ ( i -- val ) cells arr + @ ;
: main
  7 0 a!  3 1 a!  7 2 a!  5 3 a!  1 4 a!  7 5 a!  9 6 a!  2 7 a!
  0
  8 0 do
    i a@ 7 = if 1+ then
  loop
  . cr ;
