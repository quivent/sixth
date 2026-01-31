\ expect: 33
\ Compute ((3+4)*5-2) using explicit stack discipline
\ 3 4 + => 7, 7 5 * => 35, 35 2 - => 33
: main
  3 4 +     \ 7
  5 *       \ 35
  2 -       \ 33
  . cr ;
