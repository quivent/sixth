\ expected: 60000
\ Longest common subsequence, 10K pairs of strings (length 8)
\ Checksum: sum of all LCS lengths

create s1 16 allot
create s2 16 allot
create dp 81 cells allot

: max2 ( a b -- max ) 2dup < if swap then drop ;
: dp@ ( i j -- val ) 9 * + cells dp + @ ;
: dp! ( val i j -- ) 9 * + cells dp + ! ;

: fill-s1 ( n -- ) 8 0 do i n + 26 mod 97 + s1 i + c! loop ;
: fill-s2 ( n -- ) 8 0 do i n 2 + + 26 mod 97 + s2 i + c! loop ;

: lcs-len ( -- len )
  9 0 do 0 0 i dp! 0 i 0 dp! loop
  8 0 do
    8 0 do
      s1 j + c@ s2 i + c@ = if
        j i dp@ 1+ j 1+ i 1+ dp!
      else
        j 1+ i dp@ j i 1+ dp@ max2 j 1+ i 1+ dp!
      then
    loop
  loop
  8 8 dp@ ;

: bench-lcs ( -- sum )
  0
  10000 0 do
    i fill-s1
    i fill-s2
    lcs-len +
  loop ;

: main bench-lcs . cr ;
