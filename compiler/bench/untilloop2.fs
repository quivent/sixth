\ expected: 1000000000
\ UNTIL with 0= condition, 1B iterations
: main
  0 1000000000
  begin
    swap 1+ swap
    1-
    dup 0=
  until
  drop . cr
;
