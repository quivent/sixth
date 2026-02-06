\ expect: 0
\ ADVERSARIAL: Fill with zero, verify all bytes cleared
\ Edge case: fill value of 0
: main
  here           \ save start
  16 allot
  dup 16 255 fill   \ first fill with 255 to ensure non-zero
  dup 16 0 fill     \ then fill with 0

  \ OR all bytes together - should be 0 if all are 0
  dup c@
  over 1+ c@ or
  over 2 + c@ or
  over 3 + c@ or
  over 4 + c@ or
  over 5 + c@ or
  over 6 + c@ or
  over 7 + c@ or
  over 8 + c@ or
  over 9 + c@ or
  over 10 + c@ or
  over 11 + c@ or
  over 12 + c@ or
  over 13 + c@ or
  over 14 + c@ or
  swap 15 + c@ or   \ result = 0 if all were 0
;
