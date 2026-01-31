\ expect: 77
\ Find maximum in array of 6 elements
create arr 48 allot
: a! ( val i -- ) cells arr + ! ;
: a@ ( i -- val ) cells arr + @ ;
: main
  23 0 a!  77 1 a!  5 2 a!  42 3 a!  18 4 a!  61 5 a!
  arr @
  6 1 do
    i a@ max
  loop
  . cr ;
