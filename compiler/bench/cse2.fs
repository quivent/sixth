\ expected: 900000000
\ More complex CSE - same expensive computation multiple times

: expensive ( n -- n ) dup * dup * ;

: process ( n -- result )
  expensive   \ n^4
  dup +       \ 2*n^4
  swap drop ;

: main
  0 100000000 0 do
    3 process drop 9 +
  loop . cr ;
