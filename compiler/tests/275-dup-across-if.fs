\ expect: 15
\ Test: dup value used after if → 15
: main 5 dup 3 > if 10 + then . cr ;
