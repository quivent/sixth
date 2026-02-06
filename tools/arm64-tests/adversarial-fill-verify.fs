\ expect: 1
\ ADVERSARIAL: Verify multiple contiguous bytes filled
\ Tests that ALL bytes in range are filled, not just endpoints
\ Catches stride errors or partial fills

: main
  here 10 allot     \ allocate 10 bytes
  here 10 -         \ start address

  \ First fill with 0 to clear
  dup 10 0 fill

  \ Fill with 77
  dup 10 77 fill

  \ Check ALL 10 bytes
  dup     c@ 77 =   \ byte 0
  over 1 + c@ 77 = and  \ byte 1
  over 2 + c@ 77 = and  \ byte 2
  over 3 + c@ 77 = and  \ byte 3
  over 4 + c@ 77 = and  \ byte 4
  over 5 + c@ 77 = and  \ byte 5
  over 6 + c@ 77 = and  \ byte 6
  over 7 + c@ 77 = and  \ byte 7
  over 8 + c@ 77 = and  \ byte 8
  over 9 + c@ 77 = and  \ byte 9
  swap drop
  if 1 else 0 then
;
