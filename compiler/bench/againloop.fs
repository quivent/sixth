\ expected: 1000000000
\ BEGIN/AGAIN with EXIT, 1B iterations
: main
  0 0
  begin
    swap 1+ swap
    1+
    dup 1000000000 >= if drop . cr exit then
  again
;
