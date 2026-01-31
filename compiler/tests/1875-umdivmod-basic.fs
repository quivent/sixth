\ expect: 4 1
\ 13 / 3 = quot 4, rem 1. um/mod ( ud-lo ud-hi u -- rem quot )
\ TOS=quot printed first
: main 13 0 3 um/mod . . cr ;
