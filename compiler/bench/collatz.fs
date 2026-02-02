\ expected: 1637505
\ Collatz sequence stopping time sum benchmark

: collatz-len ( n -- steps )
  0 swap
  begin dup 1 > while
    dup 2 mod 0= if
      2/
    else
      3 * 1+
    then
    swap 1+ swap
  repeat drop ;

: main 0 100001 1 do i collatz-len + loop . cr ;
