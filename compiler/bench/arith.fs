\ expected: 4294967040
\ Pure arithmetic stress - no memory, no calls

: main
  0 1000000000 0 do i 3 * 7 + $FF and + loop . cr ;
