\ expect: -5 2 -4 -1
\ Compare fm/mod vs sm/rem for -13/3
\ fm/mod: quot=-5, rem=2. sm/rem: quot=-4, rem=-1
: main -13 -1 3 fm/mod . . -13 -1 3 sm/rem . . cr ;
