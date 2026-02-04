\ expected: 7771200
\ 64x64 matrix multiply, 100 times
\ Checksum: sum of C[0][0] from each multiply

create A 4096 cells allot
create B 4096 cells allot
create C 4096 cells allot

: mat@ ( mat i j -- val ) 64 * + cells + @ ;
: mat! ( val mat i j -- ) 64 * + cells + ! ;

: init-mats ( iter -- )
  4096 0 do
    dup i + 100 mod A i cells + !
    dup i + 50 mod B i cells + !
  loop drop
  4096 0 do 0 C i cells + ! loop ;

: matmul ( -- )
  64 0 do
    64 0 do
      0
      64 0 do
        A j i mat@ B i k mat@ * +
      loop
      C j k mat!
    loop
  loop ;

: bench-matmul64 ( -- sum )
  0
  100 0 do
    i init-mats
    matmul
    C @ +
  loop ;

: main bench-matmul64 . cr ;
