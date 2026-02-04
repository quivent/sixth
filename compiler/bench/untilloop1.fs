\ expected: 1000000000
\ BEGIN/UNTIL, 1B iterations
: main
  0 0
  begin
    swap 1+ swap
    1+
    dup 1000000000 >=
  until
  drop . cr
;
