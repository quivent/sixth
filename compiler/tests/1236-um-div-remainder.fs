\ expect: 1 3
\ 7 / 4 = 1 remainder 3. TOS=quot printed first, then rem.
: main 7 0 4 um/mod . . cr ;
