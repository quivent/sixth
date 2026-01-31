\ expect: 4 1
\ 13 / 3 = quot 4, rem 1 (symmetric). sm/rem ( d-lo d-hi n -- rem quot )
: main 13 0 3 sm/rem . . cr ;
