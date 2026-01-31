\ expect: 99
\ Test 946: if branch exits early, else runs normally
: foo 1 if 99 . cr exit else 0 . cr then ;
: main foo ;
