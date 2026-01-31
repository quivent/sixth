\ expect: 4 5 1
\ Test 937: interleave . with arithmetic on deep stack
\ 1 2 3 4 -> print 4, now 1 2 3 -> + -> 1 5 -> print 5, now 1 -> print 1
: main 1 2 3 4 . + . . cr ;
