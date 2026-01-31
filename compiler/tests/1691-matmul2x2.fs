\ expect: 19 22 43 50
\ 2x2 matrix multiply: [[1,2],[3,4]] * [[5,6],[7,8]]
\ Result: [[1*5+2*7, 1*6+2*8], [3*5+4*7, 3*6+4*8]]
\ = [[19, 22], [43, 50]]
create A 32 allot
create B 32 allot
create C 32 allot
: m! ( val mat idx -- ) cells + ! ;
: m@ ( mat idx -- val ) cells + @ ;
: main
  1 A 0 m!  2 A 1 m!  3 A 2 m!  4 A 3 m!
  5 B 0 m!  6 B 1 m!  7 B 2 m!  8 B 3 m!
  \ C[0] = A[0]*B[0] + A[1]*B[2]
  A 0 m@ B 0 m@ *  A 1 m@ B 2 m@ *  +  C 0 m!
  \ C[1] = A[0]*B[1] + A[1]*B[3]
  A 0 m@ B 1 m@ *  A 1 m@ B 3 m@ *  +  C 1 m!
  \ C[2] = A[2]*B[0] + A[3]*B[2]
  A 2 m@ B 0 m@ *  A 3 m@ B 2 m@ *  +  C 2 m!
  \ C[3] = A[2]*B[1] + A[3]*B[3]
  A 2 m@ B 1 m@ *  A 3 m@ B 3 m@ *  +  C 3 m!
  4 0 do C i m@ . loop cr ;
