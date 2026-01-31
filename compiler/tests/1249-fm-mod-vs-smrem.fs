\ expect: -4 2 -3 -1
\ Compare fm/mod vs sm/rem for -10/3
\ fm/mod: quot=-4, rem=2. sm/rem: quot=-3, rem=-1
: main -10 -1 3 fm/mod . . -10 -1 3 sm/rem . . cr ;
