\ expect: 1x3 2x2 3x1 4x3 5x1
\ Run-length encode [1 1 1 2 2 3 4 4 4 5]
create arr 80 allot
: arr@ ( i -- val ) 8 * arr + @ ;
: arr! ( val i -- ) 8 * arr + ! ;
variable cur
variable cnt
: print-run ( -- )
  cur @ 48 + emit 120 emit cnt @ 48 + emit 32 emit ;
: rle ( n -- )
  0 arr@ cur !  1 cnt !
  1 do
    i arr@ cur @ = if
      cnt @ 1+ cnt !
    else
      print-run
      i arr@ cur !  1 cnt !
    then
  loop
  print-run ;
: main
  1 0 arr!  1 1 arr!  1 2 arr!  2 3 arr!  2 4 arr!
  3 5 arr!  4 6 arr!  4 7 arr!  4 8 arr!  5 9 arr!
  10 rle cr ;
