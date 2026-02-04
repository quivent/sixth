\ expected: 1330170
\ 128x128 matrix multiply, 10 times
\ Checksum: sum of C[0][0] from each multiply

create A 16384 cells allot
create B 16384 cells allot
create C 16384 cells allot

: mat@ ( mat i j -- val ) 128 * + cells + @ ;
: mat! ( val mat i j -- ) 128 * + cells + ! ;

: init-mats ( iter -- )
  16384 0 do
    dup i + 100 mod A i cells + !
    dup i + 50 mod B i cells + !
  loop drop
  16384 0 do 0 C i cells + ! loop ;

: matmul ( -- )
  128 0 do
    128 0 do
      0
      128 0 do
        A j i mat@ B i k mat@ * +
      loop
      C j k mat!
    loop
  loop ;

: bench-matmul128 ( -- sum )
  0
  10 0 do
    i init-mats
    matmul
    C @ +
  loop ;

: main bench-matmul128 . cr ;
