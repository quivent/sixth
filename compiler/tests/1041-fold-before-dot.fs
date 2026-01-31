\ expect: 42
\ Test 1041: ct-flush before I/O (.)
\ REGRESSION: . must flush ct-stack so folded value prints correctly.
: main 6 7 * . cr ;
