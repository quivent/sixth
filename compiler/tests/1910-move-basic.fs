\ expect: 72 101 108
create src 8 allot
create dst 8 allot
: main ( -- )
  72 src c!
  101 src 1 + c!
  108 src 2 + c!
  src dst 3 move
  dst c@ . dst 1 + c@ . dst 2 + c@ . cr ;
