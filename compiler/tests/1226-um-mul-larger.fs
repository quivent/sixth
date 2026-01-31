\ expect: 0 1000000
\ 1000 * 1000 = 1000000, still fits in low word
: main 1000 1000 um* . . cr ;
