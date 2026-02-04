\ expected: 1000000000
\ 1- dup 0> while, 1B iterations - tests loop elimination
: main
  0 1000000000
  begin
    1-
    swap 1+ swap
    dup 0>
  while repeat
  drop . cr
;
