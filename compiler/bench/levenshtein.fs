\ expected: 60000
\ Edit distance, 10K pairs of short strings (length 8)
\ Checksum: sum of all distances

create s1 16 allot
create s2 16 allot
create dp 81 cells allot

: min2 ( a b -- min ) 2dup > if swap then drop ;
: min3 ( a b c -- min ) min2 min2 ;
: dp@ ( i j -- val ) 9 * + cells dp + @ ;
: dp! ( val i j -- ) 9 * + cells dp + ! ;

: fill-s1 ( n -- ) 8 0 do i n + 26 mod 97 + s1 i + c! loop ;
: fill-s2 ( n -- ) 8 0 do i n 3 + + 26 mod 97 + s2 i + c! loop ;

: levenshtein ( -- dist )
  9 0 do i 0 i dp! loop
  9 0 do 0 i 0 dp! loop
  8 0 do
    8 0 do
      s1 j + c@ s2 i + c@ = if
        j i dp@ j 1+ i 1+ dp!
      else
        j i dp@ 1+
        j 1+ i dp@ 1+
        j i 1+ dp@ 1+
        min3 j 1+ i 1+ dp!
      then
    loop
  loop
  8 8 dp@ ;

: bench-lev ( -- sum )
  0
  10000 0 do
    i fill-s1
    i fill-s2
    levenshtein +
  loop ;

: main bench-lev . cr ;
