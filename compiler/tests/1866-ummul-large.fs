\ expect: 0 1000000000
\ 50000 * 20000 = 1000000000, still fits in single cell
: main 50000 20000 um* . . cr ;
