\ expect: 6 2 6 2 0
: main
  1000
  begin dup 0> while
    s>d 7 fm/mod
    swap .
  repeat
  . cr ;
