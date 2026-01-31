\ expect: 32
create avec 24 allot
create bvec 24 allot
: a@ ( i -- val ) 8 * avec + @ ;
: b@ ( i -- val ) 8 * bvec + @ ;
: dot ( n -- sum )
  0 swap
  0 do
    i a@ i b@ * +
  loop ;
: main
  1 avec !  2 avec 8 + !  3 avec 16 + !
  4 bvec !  5 bvec 8 + !  6 bvec 16 + !
  3 dot . cr ;
