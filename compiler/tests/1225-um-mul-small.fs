\ expect: 0 15
\ 3 * 5 = 15, fits in low word, high = 0
: main 3 5 um* . . cr ;
