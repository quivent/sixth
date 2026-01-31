\ expect: 3 1
\ 10 / 3 = 3 remainder 1 (same as sm/rem for positive). TOS=quot first.
: main 10 0 3 fm/mod . . cr ;
