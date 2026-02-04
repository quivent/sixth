\ expected: 12
\ Roman numerals benchmark - max len of roman 1-1000, run 500 times
\ Max is 888 = DCCCLXXXVIII = 12 chars

: roman-len ( n -- len )
  0 swap
  begin dup 1000 >= while 1000 - swap 1+ swap repeat
  dup 900 >= if 900 - swap 2 + swap then
  dup 500 >= if 500 - swap 1+ swap then
  dup 400 >= if 400 - swap 2 + swap then
  begin dup 100 >= while 100 - swap 1+ swap repeat
  dup 90 >= if 90 - swap 2 + swap then
  dup 50 >= if 50 - swap 1+ swap then
  dup 40 >= if 40 - swap 2 + swap then
  begin dup 10 >= while 10 - swap 1+ swap repeat
  dup 9 >= if 9 - swap 2 + swap then
  dup 5 >= if 5 - swap 1+ swap then
  dup 4 >= if 4 - swap 2 + swap then
  + ;

: bench-roman ( limit -- max )
  0 swap 1+ 1 do
    i roman-len max
  loop ;

: main ( -- )
  0
  500 0 do
    drop 1000 bench-roman
  loop
  . cr ;
