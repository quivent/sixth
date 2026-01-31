\ expect: 1 2 3 4 5 6
\ Merge sorted [1 3 5] and [2 4 6]
create a 24 allot
create b 24 allot
create out 48 allot
: a@ ( i -- val ) 8 * a + @ ;
: a! ( val i -- ) 8 * a + ! ;
: b@ ( i -- val ) 8 * b + @ ;
: b! ( val i -- ) 8 * b + ! ;
: o@ ( i -- val ) 8 * out + @ ;
: o! ( val i -- ) 8 * out + ! ;
variable ai  variable bi  variable oi
variable an  variable bn
: merge ( na nb -- )
  bn !  an !
  0 ai !  0 bi !  0 oi !
  begin ai @ an @ < bi @ bn @ < and while
    ai @ a@ bi @ b@ < if
      ai @ a@ oi @ o!  ai @ 1+ ai !
    else
      bi @ b@ oi @ o!  bi @ 1+ bi !
    then
    oi @ 1+ oi !
  repeat
  begin ai @ an @ < while
    ai @ a@ oi @ o!  ai @ 1+ ai !  oi @ 1+ oi !
  repeat
  begin bi @ bn @ < while
    bi @ b@ oi @ o!  bi @ 1+ bi !  oi @ 1+ oi !
  repeat ;
: main
  1 0 a!  3 1 a!  5 2 a!
  2 0 b!  4 1 b!  6 2 b!
  3 3 merge
  6 0 do i o@ . loop cr ;
