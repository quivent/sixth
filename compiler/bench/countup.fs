\ expected: 1000000000
\ 1+ dup limit < while, 1B iterations
: main
  0 0
  begin
    1+
    swap 1+ swap
    dup 1000000000 <
  while repeat
  drop . cr
;
