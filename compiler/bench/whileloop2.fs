\ expected: 500000000
\ WHILE with complex condition (i < limit AND i AND 1 = 0), 500M
: main
  0 0
  begin
    dup 1000000000 < over 1 and 0= and
  while
    swap 1+ swap
    2 +
  repeat
  drop . cr
;
