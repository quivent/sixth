\ expected: 114434624
\ Population count - count bits set in numbers

: popcount ( n -- count )
  0 swap
  begin dup while
    dup 1 and rot + swap
    1 rshift
  repeat drop ;

: main
  0
  10000000 0 do
    i popcount +
  loop
  . cr ;
