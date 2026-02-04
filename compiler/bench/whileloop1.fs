\ expected: 1000000000
\ BEGIN/WHILE/REPEAT, 1B iterations
: main
  0 0
  begin
    dup 1000000000 <
  while
    swap 1+ swap
    1+
  repeat
  drop . cr
;
