\ expect: 19 22 43 50
create a 32 allot
create b 32 allot
create c 32 allot
: m! ( val arr idx -- ) 8 * + ! ;
: m@ ( arr idx -- val ) 8 * + @ ;
: mat-mul ( -- )
  a 0 m@ b 0 m@ *  a 1 m@ b 2 m@ * +  c 0 m!
  a 0 m@ b 1 m@ *  a 1 m@ b 3 m@ * +  c 1 m!
  a 2 m@ b 0 m@ *  a 3 m@ b 2 m@ * +  c 2 m!
  a 2 m@ b 1 m@ *  a 3 m@ b 3 m@ * +  c 3 m! ;
: main
  1 a 0 m!  2 a 1 m!  3 a 2 m!  4 a 3 m!
  5 b 0 m!  6 b 1 m!  7 b 2 m!  8 b 3 m!
  mat-mul
  c 0 m@ . c 1 m@ . c 2 m@ . c 3 m@ . cr ;
