\ expect: 40
\ Test 1421: 6 values, interleaved mul then adds — deep stack arithmetic
\ 1 2 3 4 5 6 * → 30, + → 34, + → 37, + → 39, + → 40
: main 1 2 3 4 5 6 * + + + + . cr ;
