\ expect: 3 -1
\ -10 / -3: floored gives quot=3, rem=-1 (same as sm/rem when signs agree). TOS=quot first.
: main -10 -1 -3 fm/mod . . cr ;
