\ expect: 198
\ ADVERSARIAL: Move data, then verify each byte with c@
\ Tests that move copies all bytes correctly, verified individually
: main
  here           \ dst address
  s" ABC" drop   \ src address
  over           \ ( dst src dst )
  3 move         \ move 3 bytes to dst
  \ dst now contains "ABC"
  dup c@         \ 'A' = 65
  over 1+ c@ +   \ + 'B' = 66 -> 131
  swap 2 + c@ +  \ + 'C' = 67 -> 198
;
