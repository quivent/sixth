\ expect: 1 4 7 2 5 8 3 6 9
\ Transpose 3x3 matrix:
\ [1 2 3]    [1 4 7]
\ [4 5 6] => [2 5 8]
\ [7 8 9]    [3 6 9]
create mat 72 allot
create out 72 allot
: m@ ( r c -- val ) swap 3 * + 8 * mat + @ ;
: m! ( val r c -- ) swap 3 * + 8 * mat + ! ;
: o! ( val r c -- ) swap 3 * + 8 * out + ! ;
: o@ ( r c -- val ) swap 3 * + 8 * out + @ ;
: transpose
  3 0 do
    3 0 do
      j i m@ i j o!
    loop
  loop ;
: main
  1 0 0 m!  2 0 1 m!  3 0 2 m!
  4 1 0 m!  5 1 1 m!  6 1 2 m!
  7 2 0 m!  8 2 1 m!  9 2 2 m!
  transpose
  3 0 do
    3 0 do j i o@ . loop
  loop cr ;
