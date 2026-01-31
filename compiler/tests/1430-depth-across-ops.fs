\ expect: 6 15
\ Test 1430: depth tracking — print from depth 6, then reduce remainder
\ 1 2 3 4 5 6 → . prints 6 → stack: 1 2 3 4 5
\ + → 9, + → 12, + → 14, + → 15
: main 1 2 3 4 5 6 . + + + + . cr ;
