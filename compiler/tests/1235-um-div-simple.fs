\ expect: 5 0
\ 10 / 2 = 5 remainder 0. um/mod ( udlo udhi u -- rem quot )
\ TOS=quot printed first by .
: main 10 0 2 um/mod . . cr ;
