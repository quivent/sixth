\ expect: -4 2
\ -10 / 3: floored gives quot=-4, rem=2 (since -4*3 + 2 = -10). TOS=quot first.
: main -10 -1 3 fm/mod . . cr ;
