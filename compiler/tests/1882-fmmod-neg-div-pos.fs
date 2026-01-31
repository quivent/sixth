\ expect: -5 2
\ -13 / 3: floored gives quot=-5, rem=2 (since -5*3 + 2 = -13)
\ rem has sign of divisor
: main -13 -1 3 fm/mod . . cr ;
